# Reimporta la BD antigua de Conex (Conex_bs.mdb) al modelo nuevo, INCLUIDA LA LISTA DE ESPERA,
# y sustituye la BD que usa la aplicacion.
#
# Lista de espera en el programa antiguo (consultas "Espera" y "AlumnesGrupCon" de Conex.mdb):
#  - Una fila de AlumnesGrup con Espera = True es una persona EN ESPERA, no inscrita:
#      * si esta en una clase normal, espera para esa clase;
#      * si esta en el grupo "LLISTA D'ESPERA" (IdGrup 101), espera para la clase indicada
#        por sus columnas Curs (nombre del curso) + IdHorari (dia y turno).
#  - El grupo "LLISTA D'ESPERA" NO es una clase: no se importa como actividad.
#  - Bagul = True es otra lista distinta ("Llista del Bagul"). Por defecto NO se pasa a la lista de
#    espera; con -IncludeBagul si (se resuelve igual que las filas en espera).
#  - Las demas filas del grupo 101 (ni Espera ni Bagul) son historicas y se descartan.
#
# Seguridad:
#  - La BD Access se abre en solo lectura.
#  - La BD nueva se construye en una carpeta temporal con Infrastructure\Data\Schema\schema.sql
#    (el mismo esquema que usa la app) y se valida (integridad, claves ajenas, recuentos) antes de usarla.
#  - Si la BD actual tiene cambios hechos desde la app que la reimportacion perderia
#    (usuarios, clases, inscripciones, renovaciones, listas de espera...), se detiene y los enumera.
#    -Force continua igualmente.
#  - Se conservan la configuracion (AppSettings) y las plazas maximas de cada clase (MaxStudents).
#  - Antes de sustituirla se hace copia de seguridad de la BD actual en <carpeta de datos>\backups.
#  - Se deja un informe con todo lo que no se ha podido asignar en <carpeta de datos>\reimportacion_<fecha>.txt
#
# Este script es ASCII a proposito (PowerShell 5 lee los .ps1 sin BOM como ANSI).
#
# Uso:
#   powershell -ExecutionPolicy Bypass -File tools\reimport_db.ps1 -DryRun     (solo genera y valida, no toca nada)
#   powershell -ExecutionPolicy Bypass -File tools\reimport_db.ps1             (reimporta)
# Parametros opcionales: -MdbPath <ruta> -DataDir <carpeta> -Sqlite <ruta sqlite3.exe>
#                        -IncludeBagul -WaitlistGroupId <ids> -Force
[CmdletBinding()]
param(
    [string]$MdbPath = "F:\CONEX - PROGRAMA PC\CONEX - PROGRAMA PC\Conex_bs.mdb",
    # Carpeta de datos de la app. Por defecto la misma que usa la app (DatabasePaths.DataDirectory).
    [string]$DataDir = "",
    [string]$Sqlite = "sqlite3",
    # Tambien pasa a la lista de espera las filas del "bagul" del grupo de lista de espera.
    [switch]$IncludeBagul,
    # Ids de grupo que son la lista de espera. Por defecto se detectan por el nombre del curso.
    [int[]]$WaitlistGroupId = @(),
    # Solo genera y valida la BD nueva y el informe; no sustituye nada.
    [switch]$DryRun,
    # Sustituye la BD aunque tenga cambios hechos desde la app que se perderan.
    [switch]$Force
)
$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2

$AppProcessName = "CONEX_APP"
$DbFileName = "conex_app.db"
$root = Split-Path $PSScriptRoot -Parent
$schemaPath = Join-Path $root "Infrastructure\Data\Schema\schema.sql"

if (-not $DataDir) {
    $DataDir = if ($env:CONEX_DATA_DIR) { $env:CONEX_DATA_DIR } else { Join-Path $env:LOCALAPPDATA "CONEX" }
}
$targetDb = Join-Path $DataDir $DbFileName
$backupDir = Join-Path $DataDir "backups"
$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
$reportPath = Join-Path $DataDir "reimportacion_$stamp.txt"

# ---------------------------------------------------------------------------------------------
# Lectura del .mdb y generacion del SQL (C# 5: es lo que compila Add-Type en PowerShell 5)
# ---------------------------------------------------------------------------------------------
$code = @"
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class ConexImportResult {
    public int Users, Activities, Enrollments, Renewals, Waitlist;
    public int WaitlistUnmatched, WaitlistAmbiguous;
    public string WaitlistGroupIds = "";
    public List<string> Summary = new List<string>();
    public List<string> Report = new List<string>();
}

public class ConexImporter {
    const string NoDate = "0001-01-01 00:00:00";
    const string Fmt = "yyyy-MM-dd HH:mm:ss";
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    static readonly Regex WaitlistName = new Regex(@"^(LLISTA|LISTA) (D|DE) ESPERA$");

    class Grp { public int Id; public string Name; public string Norm; public int IdHorari; public string Tutor; public bool IsWaitlist; }
    class Row { public int IdAlumne, IdUsuari, IdGrup, IdHorari; public string Curs, Horari; public bool Espera, Bagul; public DateTime? Data; }
    class WEntry { public int UserId, ActivityId, IdAlumne; public DateTime JoinedAt; public bool DateEstimated; }

    public static ConexImportResult Build(string mdbPath, string sqlPath, bool includeBagul, int[] waitlistOverride) {
        ConexImportResult res = new ConexImportResult();
        string now = DateTime.Now.ToString(Fmt, Inv);

        // Mode=Read: nunca modificamos la BD original
        string connString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Mode=Read;Data Source='{0}';", mdbPath);
        DataTable dtCursos, dtGrups, dtUsuaris, dtAlumnes, dtRebuts, dtHoraris;
        using (OleDbConnection conn = new OleDbConnection(connString)) {
            conn.Open();
            dtCursos = Fill(conn, "Cursos");
            dtGrups = Fill(conn, "Grups");
            dtUsuaris = Fill(conn, "Usuaris");
            dtAlumnes = Fill(conn, "AlumnesGrup");
            dtRebuts = Fill(conn, "Rebuts");
            dtHoraris = Fill(conn, "Horaris");
        }
        RequireColumns(dtAlumnes, "IdAlumne", "IdUsuari", "IdGrup", "Curs", "IdHorari", "Espera", "Bagul", "Data");
        RequireColumns(dtGrups, "IdGrup", "IdCurs", "IdHorari");
        RequireColumns(dtUsuaris, "IdUsuari", "Nom");

        Dictionary<string, string> cursos = new Dictionary<string, string>();
        foreach (DataRow r in dtCursos.Rows) {
            string id = Str(GetVal(r, "IdCurs"));
            if (id.Length > 0) cursos[id] = Str(GetVal(r, "Curs"));
        }
        Dictionary<int, string> horaris = new Dictionary<int, string>();
        foreach (DataRow r in dtHoraris.Rows) {
            int id = ToInt(GetVal(r, "IdHorari"));
            if (id > 0) horaris[id] = Clean(GetVal(r, "Horari"));
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("-- Generado desde " + Path.GetFileName(mdbPath) + " el " + now + " (solo datos; el esquema es schema.sql)");
        sb.AppendLine("PRAGMA foreign_keys = ON;");
        sb.AppendLine("BEGIN TRANSACTION;");

        // ---------- Users (misma conversion que import_data_mdb.ps1) ----------
        HashSet<int> validUsers = new HashSet<int>();
        HashSet<int> discharged = new HashSet<int>();
        Dictionary<int, string> userNames = new Dictionary<int, string>();
        Dictionary<int, KeyValuePair<string, string>> currentRenewal = new Dictionary<int, KeyValuePair<string, string>>();
        int usersSkipped = 0;
        foreach (DataRow r in dtUsuaris.Rows) {
            int id = ToInt(GetVal(r, "IdUsuari"));
            string nom = Clean(GetVal(r, "Nom"));
            if (id <= 0 || string.IsNullOrWhiteSpace(nom)) { usersSkipped++; continue; }
            if (!validUsers.Add(id)) throw new Exception("IdUsuari repetido en Usuaris: " + id);

            string telefon = Clean(GetVal(r, "Telefon"));
            string mobil = Clean(GetVal(r, "Mobil"));
            string phone = telefon.Length > 0 ? telefon : mobil;
            string cognom1 = Clean(GetVal(r, "Cognom1"));
            string cognom2 = Clean(GetVal(r, "Cognom2"));
            string baixa = DateOr(GetVal(r, "DBaixa"), null);

            sb.AppendLine(string.Format(
                "INSERT INTO Users (Id, Name, Surname, SecondSurname, IdCard, Address, Location, Email, Phone, IsPartner, IsTutor, CreatedAt, DischargedAt) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, '{11}', '{12}');",
                id, Q(nom), Q(cognom1), Q(cognom2), Q(Clean(GetVal(r, "Dni"))),
                Q(Clean(GetVal(r, "Adre"))), Q(Clean(GetVal(r, "Poblaci"))), Q(Clean(GetVal(r, "Mail"))), Q(phone),
                ToBool(GetVal(r, "EsSoci")), ToBool(GetVal(r, "EsEnsenyant")),
                DateOr(GetVal(r, "DAlta"), NoDate), baixa ?? NoDate));
            res.Users++;
            userNames[id] = (cognom1 + " " + cognom2 + ", " + nom).Replace("  ", " ").Trim();
            if (baixa != null) discharged.Add(id);

            string ultima = DateOr(GetVal(r, "DataRenovacio"), null);
            string nueva = DateOr(GetVal(r, "Venciment"), null);
            if (ultima == null && nueva != null) ultima = DateOr(GetVal(r, "DAlta"), null);
            if (ultima != null) currentRenewal[id] = new KeyValuePair<string, string>(ultima, nueva);
        }

        // ---------- Grupos -> Activities (excepto el grupo de lista de espera) ----------
        Dictionary<int, Grp> groups = new Dictionary<int, Grp>();
        foreach (DataRow r in dtGrups.Rows) {
            int id = ToInt(GetVal(r, "IdGrup"));
            if (id <= 0) continue;
            string idCurs = Str(GetVal(r, "IdCurs"));
            string name = cursos.ContainsKey(idCurs) ? cursos[idCurs].Trim() : "";
            if (string.IsNullOrWhiteSpace(name)) name = Clean(GetVal(r, "Curs"));
            if (string.IsNullOrWhiteSpace(name)) name = "Activity " + id;

            Grp g = new Grp { Id = id, Name = name, Norm = Norm(name), IdHorari = ToInt(GetVal(r, "IdHorari")), Tutor = Clean(GetVal(r, "Ensenyant")) };
            g.IsWaitlist = (waitlistOverride != null && waitlistOverride.Length > 0)
                ? waitlistOverride.Contains(id)
                : WaitlistName.IsMatch(g.Norm);
            groups[id] = g;
            if (g.IsWaitlist) continue;

            sb.AppendLine(string.Format(
                "INSERT INTO Activities (Id, Name, Tutor, Classroom, Date, MaxStudents, CreatedAt) VALUES ({0}, {1}, {2}, {3}, '{4}', 0, '{5}');",
                id, Q(name), Q(g.Tutor), Q(Clean(GetVal(r, "Aula"))),
                ParseDateTime(GetVal(r, "Dia1"), GetVal(r, "Hores")), now));
            res.Activities++;
        }
        if (waitlistOverride != null) {
            foreach (int id in waitlistOverride)
                if (!groups.ContainsKey(id)) throw new Exception("El grupo de lista de espera " + id + " no existe en Grups.");
        }
        List<Grp> waitlistGroups = groups.Values.Where(g => g.IsWaitlist).OrderBy(g => g.Id).ToList();
        List<Grp> classes = groups.Values.Where(g => !g.IsWaitlist).ToList();
        res.WaitlistGroupIds = string.Join(",", waitlistGroups.Select(g => g.Id.ToString(Inv)).ToArray());

        // ---------- Filas de AlumnesGrup ----------
        List<Row> rows = new List<Row>();
        foreach (DataRow r in dtAlumnes.Rows) {
            object d = GetVal(r, "Data");
            rows.Add(new Row {
                IdAlumne = ToInt(GetVal(r, "IdAlumne")), IdUsuari = ToInt(GetVal(r, "IdUsuari")), IdGrup = ToInt(GetVal(r, "IdGrup")),
                IdHorari = ToInt(GetVal(r, "IdHorari")), Curs = Clean(GetVal(r, "Curs")), Horari = Clean(GetVal(r, "Horari")),
                Espera = ToBool(GetVal(r, "Espera")) == 1, Bagul = ToBool(GetVal(r, "Bagul")) == 1,
                Data = d is DateTime ? (DateTime?)(DateTime)d : null
            });
        }
        rows.Sort((a, b) => a.IdAlumne.CompareTo(b.IdAlumne));
        // Fecha para las filas en espera sin fecha: la ultima fecha de la BD antigua (van al final de la cola)
        DateTime lastOldDate = rows.Where(x => x.Data.HasValue).Select(x => x.Data.Value).DefaultIfEmpty(DateTime.Now).Max();

        int rowsInvalidUser = 0, rowsOrphanGroup = 0, historicDiscarded = 0, bagulDiscarded = 0, bagulWouldMatch = 0;
        int fromOwnClass = 0, fromWaitlistGroup = 0, fromBagul = 0;
        HashSet<string> enrolled = new HashSet<string>();
        List<int[]> enrollments = new List<int[]>();
        List<WEntry> candidates = new List<WEntry>();
        List<string> unmatched = new List<string>();
        List<string> ambiguous = new List<string>();

        foreach (Row row in rows) {
            if (!validUsers.Contains(row.IdUsuari)) { rowsInvalidUser++; continue; }
            Grp grp;
            if (!groups.TryGetValue(row.IdGrup, out grp)) { rowsOrphanGroup++; continue; }

            if (!grp.IsWaitlist) {
                if (row.Espera) {
                    candidates.Add(NewEntry(row, grp.Id, lastOldDate));
                    fromOwnClass++;
                } else if (enrolled.Add(grp.Id + "|" + row.IdUsuari)) {
                    enrollments.Add(new int[] { grp.Id, row.IdUsuari });
                }
                continue;
            }

            bool take = row.Espera || (includeBagul && row.Bagul);
            string reason;
            List<Grp> targets = Resolve(row, classes, horaris, out reason);
            if (!take) {
                if (row.Bagul) { bagulDiscarded++; if (targets.Count > 0) bagulWouldMatch++; }
                else historicDiscarded++;
                continue;
            }
            if (row.Espera) fromWaitlistGroup++; else fromBagul++;

            if (targets.Count == 0) {
                unmatched.Add(Describe(row, userNames, horaris) + "  ->  " + reason);
                continue;
            }
            if (targets.Count > 1) {
                ambiguous.Add(Describe(row, userNames, horaris) + "  ->  en espera en " + targets.Count + " clases: " +
                    string.Join(" / ", targets.Select(t => ClassLabel(t, horaris)).ToArray()));
            }
            foreach (Grp t in targets) candidates.Add(NewEntry(row, t.Id, lastOldDate));
        }

        // Una sola entrada por usuario y clase (la mas antigua) y nunca para quien ya esta inscrito
        Dictionary<string, WEntry> best = new Dictionary<string, WEntry>();
        List<string> alreadyEnrolled = new List<string>();
        int duplicates = 0;
        foreach (WEntry e in candidates) {
            string key = e.ActivityId + "|" + e.UserId;
            if (enrolled.Contains(key)) {
                alreadyEnrolled.Add(UserLabel(e.UserId, userNames) + "  ->  ya inscrito en " + ClassLabel(groups[e.ActivityId], horaris));
                continue;
            }
            WEntry prev;
            if (best.TryGetValue(key, out prev)) {
                duplicates++;
                if (e.JoinedAt < prev.JoinedAt || (e.JoinedAt == prev.JoinedAt && prev.DateEstimated && !e.DateEstimated)) best[key] = e;
            } else best[key] = e;
        }
        List<WEntry> waitlist = best.Values.OrderBy(e => e.JoinedAt).ThenBy(e => e.IdAlumne).ThenBy(e => e.ActivityId).ToList();

        // ---------- ActivityUser ----------
        foreach (int[] en in enrollments) {
            sb.AppendLine(string.Format("INSERT INTO ActivityUser (ActivitiesId, StudentsId) VALUES ({0}, {1});", en[0], en[1]));
            res.Enrollments++;
        }

        // ---------- Renewals (historico Rebuts + renovacion actual de Usuaris) ----------
        HashSet<string> seen = new HashSet<string>();
        List<KeyValuePair<int, string>> renewals = new List<KeyValuePair<int, string>>();
        foreach (DataRow r in dtRebuts.Rows) {
            int idUsuari = ToInt(GetVal(r, "IdUsuari"));
            string fecha = DateOr(GetVal(r, "DataRenovacio"), null);
            if (!validUsers.Contains(idUsuari) || fecha == null) continue;
            if (seen.Add(idUsuari + "|" + fecha)) renewals.Add(new KeyValuePair<int, string>(idUsuari, fecha));
        }
        foreach (KeyValuePair<int, KeyValuePair<string, string>> kv in currentRenewal) {
            if (seen.Add(kv.Key + "|" + kv.Value.Key)) renewals.Add(new KeyValuePair<int, string>(kv.Key, kv.Value.Key));
        }
        renewals.Sort((a, b) => { int c = a.Key.CompareTo(b.Key); return c != 0 ? c : string.CompareOrdinal(a.Value, b.Value); });
        foreach (KeyValuePair<int, string> ren in renewals) {
            string next = "NULL";
            KeyValuePair<string, string> cur;
            if (currentRenewal.TryGetValue(ren.Key, out cur) && cur.Key == ren.Value && cur.Value != null) next = "'" + cur.Value + "'";
            sb.AppendLine(string.Format("INSERT INTO Renewals (UserId, RenewalDate, NextRenewalDate) VALUES ({0}, '{1}', {2});", ren.Key, ren.Value, next));
            res.Renewals++;
        }

        // ---------- Waitlists (en orden de llegada: el Id desempata las fechas iguales) ----------
        foreach (WEntry e in waitlist) {
            sb.AppendLine(string.Format("INSERT INTO Waitlists (UserId, ActivityId, JoinedAt) VALUES ({0}, {1}, '{2}');",
                e.UserId, e.ActivityId, e.JoinedAt.ToString(Fmt, Inv)));
            res.Waitlist++;
        }

        sb.AppendLine("COMMIT;");
        File.WriteAllText(sqlPath, sb.ToString(), new UTF8Encoding(false));

        // ---------- Resumen e informe ----------
        res.WaitlistUnmatched = unmatched.Count;
        res.WaitlistAmbiguous = ambiguous.Count;
        int estimated = waitlist.Count(e => e.DateEstimated);
        int waitlistDischarged = waitlist.Count(e => discharged.Contains(e.UserId));

        List<string> s = res.Summary;
        s.Add("Usuarios: " + res.Users + (usersSkipped > 0 ? " (" + usersSkipped + " filas sin Id o sin nombre descartadas)" : ""));
        s.Add("Clases: " + res.Activities + "   (grupo de lista de espera no importado como clase: " + (res.WaitlistGroupIds.Length > 0 ? res.WaitlistGroupIds : "NINGUNO") + ")");
        s.Add("Inscripciones: " + res.Enrollments);
        s.Add("Renovaciones: " + res.Renewals);
        s.Add("Lista de espera: " + res.Waitlist + " entradas, " + waitlist.Select(e => e.UserId).Distinct().Count() + " personas, " + waitlist.Select(e => e.ActivityId).Distinct().Count() + " clases");
        s.Add("  origen: " + fromWaitlistGroup + " filas en espera del grupo de lista de espera, " + fromOwnClass + " en espera dentro de su clase" + (includeBagul ? ", " + fromBagul + " del bagul" : ""));
        s.Add("  sin clase asignable: " + unmatched.Count + "   asignadas a varias clases: " + ambiguous.Count + "   ya inscritos: " + alreadyEnrolled.Count + "   repetidas: " + duplicates);
        if (estimated > 0) s.Add("  sin fecha en la BD antigua: " + estimated + " (se ponen al final de la cola, fecha " + lastOldDate.ToString("dd/MM/yyyy", Inv) + ")");
        if (waitlistDischarged > 0) s.Add("  usuarios dados de baja en la lista de espera: " + waitlistDischarged);
        if (!includeBagul) s.Add("Bagul NO importado: " + bagulDiscarded + " filas (" + bagulWouldMatch + " tendrian clase). Usa -IncludeBagul para incluirlas.");
        s.Add("Filas historicas descartadas del grupo de lista de espera (ni Espera ni Bagul): " + historicDiscarded);
        if (rowsInvalidUser > 0) s.Add("Filas de AlumnesGrup de usuarios inexistentes: " + rowsInvalidUser);
        if (rowsOrphanGroup > 0) s.Add("Filas de AlumnesGrup de grupos inexistentes: " + rowsOrphanGroup);
        if (waitlistGroups.Count == 0) s.Add("AVISO: no se ha encontrado el grupo de lista de espera (usa -WaitlistGroupId).");

        List<string> rep = res.Report;
        rep.Add("REIMPORTACION DE CONEX  -  " + now);
        rep.Add("Origen: " + mdbPath);
        rep.Add("");
        rep.AddRange(s);
        rep.Add("");
        rep.Add("=== LISTA DE ESPERA POR CLASE ===");
        foreach (var grpEntries in waitlist.GroupBy(e => e.ActivityId).OrderBy(x => groups[x.Key].Name).ThenBy(x => groups[x.Key].IdHorari)) {
            rep.Add(ClassLabel(groups[grpEntries.Key], horaris) + ": " + grpEntries.Count());
            int pos = 1;
            foreach (WEntry e in grpEntries)
                rep.Add("    " + (pos++) + ". " + UserLabel(e.UserId, userNames) + "  (" + e.JoinedAt.ToString("dd/MM/yyyy", Inv) + (e.DateEstimated ? ", sin fecha" : "") + (discharged.Contains(e.UserId) ? ", DADO DE BAJA" : "") + ")");
        }
        AddSection(rep, "EN ESPERA PERO SIN CLASE ASIGNABLE (no se han importado; hay que apuntarlos a mano)", unmatched);
        AddSection(rep, "EN ESPERA PARA UN CURSO Y HORARIO CON VARIAS CLASES (se han puesto en espera en todas)", ambiguous);
        AddSection(rep, "EN ESPERA DE UNA CLASE EN LA QUE YA ESTAN INSCRITOS (no se han puesto en espera)", alreadyEnrolled);
        return res;
    }

    static List<Grp> Resolve(Row row, List<Grp> classes, Dictionary<int, string> horaris, out string reason) {
        reason = "";
        string norm = Norm(row.Curs);
        if (norm.Length == 0) { reason = "la fila no indica el curso"; return new List<Grp>(); }
        List<Grp> sameCourse = classes.Where(g => g.Norm == norm).OrderBy(g => g.Id).ToList();
        if (sameCourse.Count == 0) { reason = "no queda ninguna clase de este curso"; return sameCourse; }
        if (row.IdHorari > 0) {
            List<Grp> m = sameCourse.Where(g => g.IdHorari == row.IdHorari).ToList();
            if (m.Count == 0) reason = "no hay clase de este curso en ese horario (hay: " + string.Join(" / ", sameCourse.Select(g => HorariName(g.IdHorari, horaris)).ToArray()) + ")";
            return m;
        }
        if (sameCourse.Count == 1) return sameCourse;
        reason = "no indica horario y hay " + sameCourse.Count + " clases de este curso";
        return new List<Grp>();
    }

    static WEntry NewEntry(Row row, int activityId, DateTime fallback) {
        return new WEntry { UserId = row.IdUsuari, ActivityId = activityId, IdAlumne = row.IdAlumne,
            JoinedAt = row.Data.HasValue ? row.Data.Value : fallback, DateEstimated = !row.Data.HasValue };
    }

    static void AddSection(List<string> rep, string title, List<string> lines) {
        rep.Add("");
        rep.Add("=== " + title + ": " + lines.Count + " ===");
        foreach (string l in lines) rep.Add("    " + l);
    }

    static string Describe(Row row, Dictionary<int, string> userNames, Dictionary<int, string> horaris) {
        string horari = row.IdHorari > 0 ? HorariName(row.IdHorari, horaris) : (row.Horari.Length > 0 ? row.Horari : "sin horario");
        return UserLabel(row.IdUsuari, userNames) + "  |  " + (row.Curs.Length > 0 ? row.Curs : "(sin curso)") + "  |  " + horari +
            "  |  " + (row.Data.HasValue ? row.Data.Value.ToString("dd/MM/yyyy", Inv) : "sin fecha") + (row.Bagul && !row.Espera ? "  |  bagul" : "") +
            "  |  IdAlumne " + row.IdAlumne;
    }

    static string UserLabel(int id, Dictionary<int, string> userNames) {
        string n;
        return (userNames.TryGetValue(id, out n) ? n : "?") + " [" + id + "]";
    }

    static string ClassLabel(Grp g, Dictionary<int, string> horaris) {
        return g.Name + " - " + HorariName(g.IdHorari, horaris) + (g.Tutor.Length > 0 ? " - " + g.Tutor : "") + " [" + g.Id + "]";
    }

    static string HorariName(int id, Dictionary<int, string> horaris) {
        string h;
        return horaris.TryGetValue(id, out h) ? h : "horario " + id;
    }

    static DataTable Fill(OleDbConnection conn, string table) {
        DataTable dt = new DataTable();
        using (OleDbDataAdapter da = new OleDbDataAdapter("SELECT * FROM [" + table + "]", conn)) da.Fill(dt);
        return dt;
    }

    static void RequireColumns(DataTable dt, params string[] names) {
        foreach (string n in names)
            if (!dt.Columns.Contains(n)) throw new Exception("Falta la columna " + n + " en la tabla del .mdb (" + dt.TableName + ")");
    }

    // Nombre de curso comparable: sin acentos, mayusculas, solo letras y numeros separados por un espacio
    static string Norm(string s) {
        if (string.IsNullOrEmpty(s)) return "";
        string d = s.Normalize(NormalizationForm.FormD);
        StringBuilder b = new StringBuilder();
        foreach (char c in d) {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            b.Append(char.IsLetterOrDigit(c) ? char.ToUpperInvariant(c) : ' ');
        }
        return Regex.Replace(b.ToString(), @"\s+", " ").Trim();
    }

    public static object GetVal(DataRow r, string colPattern) {
        foreach (DataColumn c in r.Table.Columns)
            if (c.ColumnName.Equals(colPattern, StringComparison.OrdinalIgnoreCase)) return r[c];
        // Prefijo para columnas con acento (Adreca, Poblacio)
        foreach (DataColumn c in r.Table.Columns)
            if (c.ColumnName.StartsWith(colPattern, StringComparison.OrdinalIgnoreCase)) return r[c];
        return DBNull.Value;
    }

    static string Str(object val) {
        return (val == null || val == DBNull.Value) ? "" : Convert.ToString(val, Inv).Trim();
    }

    static int ToInt(object val) {
        int i;
        return int.TryParse(Str(val), NumberStyles.Integer, Inv, out i) ? i : 0;
    }

    // Texto limpio: sin espacios sobrantes ni saltos de linea
    static string Clean(object val) {
        return Regex.Replace(Str(val), @"[\r\n\t]+", " ").Trim();
    }

    static string Q(string s) {
        return "'" + (s ?? "").Replace("'", "''") + "'";
    }

    static string DateOr(object val, string fallback) {
        if (val is DateTime) return ((DateTime)val).ToString(Fmt, Inv);
        DateTime d;
        string s = Str(val);
        if (s.Length > 0 && DateTime.TryParse(s, out d)) return d.ToString(Fmt, Inv);
        return fallback;
    }

    static string ParseDateTime(object dateVal, object timeVal) {
        DateTime d;
        if (dateVal is DateTime) d = (DateTime)dateVal;
        else if (!DateTime.TryParse(Str(dateVal), out d)) d = new DateTime(2000, 1, 1);
        d = d.Date;

        string timeStr = Str(timeVal).ToLowerInvariant();
        if (timeStr.Length > 0) {
            Match m = Regex.Match(timeStr, @"(\d{1,2})[,.:](\d{2})");
            if (!m.Success) m = Regex.Match(timeStr, @"(\d{1,2})");
            if (m.Success) {
                int hours = int.Parse(m.Groups[1].Value, Inv);
                int minutes = (m.Groups.Count > 2 && m.Groups[2].Success) ? int.Parse(m.Groups[2].Value, Inv) : 0;
                if (hours < 24 && minutes < 60) d = d.AddHours(hours).AddMinutes(minutes);
            }
        }
        return d.ToString(Fmt, Inv);
    }

    static int ToBool(object val) {
        if (val is bool) return ((bool)val) ? 1 : 0;
        string s = Str(val).ToLowerInvariant();
        return (s == "true" || s == "1" || s == "-1" || s == "si" || s == "s") ? 1 : 0;
    }
}
"@

# ---------------------------------------------------------------------------------------------
# Utilidades
# ---------------------------------------------------------------------------------------------
function Write-Step([string]$text) { Write-Host "-> $text" -ForegroundColor Cyan }

function ConvertTo-SqlitePath([string]$path) { return $path.Replace('\', '/').Replace("'", "''") }

# Ejecuta sqlite3 y devuelve sus lineas de salida. Falla si sqlite3 devuelve error.
# Las sentencias van como argumentos: no deben contener comillas dobles (PowerShell 5 las rompe).
function Invoke-Sqlite([string]$Db, [string[]]$Commands) {
    foreach ($c in $Commands) { if ($c.Contains('"')) { throw "Sentencia SQL con comillas dobles: $c" } }
    $previous = $ErrorActionPreference
    $ErrorActionPreference = "Continue"   # en PS 5 el stderr de un .exe con 2>&1 seria un error terminante
    try {
        $out = & $Sqlite -batch -bail -noheader $Db @Commands 2>&1
        $code = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $previous
    }
    $lines = @($out | ForEach-Object { "$_" })
    if ($code -ne 0) { throw "sqlite3 ha fallado (codigo $code) en $Db`n$($lines -join "`n")" }
    return $lines
}

function Get-SqliteScalars([string]$Db, [string[]]$Queries, [string[]]$Prefix = @()) {
    $lines = @(Invoke-Sqlite $Db (@($Prefix) + @($Queries)))
    if ($lines.Count -ne $Queries.Count) { throw "Respuesta inesperada de sqlite3: $($lines -join ' | ')" }
    return $lines
}

function Test-FileLocked([string]$path) {
    if (-not (Test-Path $path)) { return $false }
    try { $fs = [IO.File]::Open($path, 'Open', 'ReadWrite', 'None'); $fs.Close(); return $false } catch { return $true }
}

# ---------------------------------------------------------------------------------------------
# 1. Comprobaciones previas
# ---------------------------------------------------------------------------------------------
Write-Step "Comprobando requisitos"
if (-not (Test-Path -LiteralPath $MdbPath)) { throw "No existe la BD antigua: $MdbPath" }
if (-not (Test-Path -LiteralPath $schemaPath)) { throw "No existe el esquema: $schemaPath" }
if (-not (Get-Command $Sqlite -ErrorAction SilentlyContinue)) { throw "No se encuentra sqlite3 ('$Sqlite'). Indica la ruta con -Sqlite." }
$sqliteVersion = (Invoke-Sqlite ":memory:" @("SELECT sqlite_version();")) -join ""
Write-Host "   sqlite3 $sqliteVersion"
Write-Host "   BD de la app: $targetDb"

$currentExists = Test-Path -LiteralPath $targetDb
if (-not $DryRun) {
    if (Get-Process -Name $AppProcessName -ErrorAction SilentlyContinue) { throw "La aplicacion CONEX esta abierta. Cierrala y vuelve a ejecutar el script." }
    if (Test-FileLocked $targetDb) { throw "La BD esta en uso por otro programa: $targetDb" }
}

$workDir = Join-Path ([IO.Path]::GetTempPath()) "conex_reimport_$stamp"
New-Item -ItemType Directory -Force -Path $workDir | Out-Null
$sqlFile = Join-Path $workDir "import_data.sql"
$newDb = Join-Path $workDir $DbFileName
$keepWorkDir = $DryRun.IsPresent

# Pasos 2 a 6. Devuelve el codigo de salida del script.
function Invoke-Reimport {
    # -----------------------------------------------------------------------------------------
    # 2. Leer el .mdb y generar el SQL
    # -----------------------------------------------------------------------------------------
    Write-Step "Leyendo $MdbPath"
    Add-Type -TypeDefinition $code -ReferencedAssemblies "System.Data", "System.Xml", "System.Core"
    try {
        $result = [ConexImporter]::Build($MdbPath, $sqlFile, $IncludeBagul.IsPresent, [int[]]$WaitlistGroupId)
    } catch {
        $msg = $_.Exception.ToString()
        if ($msg -match "ACE\.OLEDB|no est. registrado|not registered") {
            throw "No se encuentra el proveedor Microsoft.ACE.OLEDB.12.0 para esta version de PowerShell. Prueba con la de 32 bits: C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe`n$msg"
        }
        throw
    }
    $result.Summary | ForEach-Object { Write-Host "   $_" }
    $wlIds = if ($result.WaitlistGroupIds) { $result.WaitlistGroupIds } else { "-1" }

    # -----------------------------------------------------------------------------------------
    # 3. Construir la BD nueva (schema.sql de la app + datos) y validarla
    # -----------------------------------------------------------------------------------------
    Write-Step "Construyendo la BD nueva"
    Invoke-Sqlite $newDb @(".read '$(ConvertTo-SqlitePath $schemaPath)'", ".read '$(ConvertTo-SqlitePath $sqlFile)'") | Out-Null

    $checks = Get-SqliteScalars $newDb @(
        "PRAGMA integrity_check;",
        "SELECT COUNT(*) FROM pragma_foreign_key_check;",
        "SELECT COUNT(*) FROM Users;",
        "SELECT COUNT(*) FROM Activities;",
        "SELECT COUNT(*) FROM ActivityUser;",
        "SELECT COUNT(*) FROM Renewals;",
        "SELECT COUNT(*) FROM Waitlists;",
        "SELECT COUNT(*) FROM Activities WHERE Id IN ($wlIds);",
        "SELECT COUNT(*) FROM Waitlists w JOIN ActivityUser au ON au.ActivitiesId = w.ActivityId AND au.StudentsId = w.UserId;"
    )
    $expected = @("ok", "0", "$($result.Users)", "$($result.Activities)", "$($result.Enrollments)", "$($result.Renewals)", "$($result.Waitlist)", "0", "0")
    $labels = @("integridad", "claves ajenas rotas", "usuarios", "clases", "inscripciones", "renovaciones", "lista de espera", "grupo de lista de espera como clase", "inscritos y en espera a la vez")
    for ($i = 0; $i -lt $expected.Count; $i++) {
        if ($checks[$i] -ne $expected[$i]) { throw "Validacion fallida ($($labels[$i])): se esperaba '$($expected[$i])' y hay '$($checks[$i])'" }
    }
    Write-Host "   Validacion correcta" -ForegroundColor Green

    # -----------------------------------------------------------------------------------------
    # 4. Comparar con la BD actual: cambios de la app que se perderian + conservar configuracion
    # -----------------------------------------------------------------------------------------
    $lost = @()
    if ($currentExists) {
        Write-Step "Comparando con la BD actual"
        $attach = "ATTACH '$(ConvertTo-SqlitePath $targetDb)' AS cur;"
        $uCols = "Id, Name, Email, CreatedAt, Address, DischargedAt, IdCard, IsPartner, Location, Phone, SecondSurname, Surname, IsTutor"
        $aCols = "Id, Name, Tutor, Classroom, Date"
        $diff = @(
            @("usuarios creados en la app", "SELECT COUNT(*) FROM cur.Users WHERE Id NOT IN (SELECT Id FROM main.Users);"),
            @("usuarios modificados en la app", "SELECT COUNT(*) FROM (SELECT $uCols FROM cur.Users WHERE Id IN (SELECT Id FROM main.Users) EXCEPT SELECT $uCols FROM main.Users);"),
            @("usuarios borrados en la app", "SELECT COUNT(*) FROM main.Users WHERE Id NOT IN (SELECT Id FROM cur.Users);"),
            @("clases creadas en la app", "SELECT COUNT(*) FROM cur.Activities WHERE Id NOT IN (SELECT Id FROM main.Activities) AND Id NOT IN ($wlIds);"),
            @("clases modificadas en la app", "SELECT COUNT(*) FROM (SELECT $aCols FROM cur.Activities WHERE Id IN (SELECT Id FROM main.Activities) EXCEPT SELECT $aCols FROM main.Activities);"),
            @("clases borradas en la app", "SELECT COUNT(*) FROM main.Activities WHERE Id NOT IN (SELECT Id FROM cur.Activities);"),
            @("inscripciones hechas en la app", "SELECT COUNT(*) FROM (SELECT ActivitiesId, StudentsId FROM cur.ActivityUser WHERE ActivitiesId NOT IN ($wlIds) EXCEPT SELECT ActivitiesId, StudentsId FROM main.ActivityUser EXCEPT SELECT ActivityId, UserId FROM main.Waitlists);"),
            @("bajas de clase hechas en la app", "SELECT COUNT(*) FROM (SELECT ActivitiesId, StudentsId FROM main.ActivityUser EXCEPT SELECT ActivitiesId, StudentsId FROM cur.ActivityUser);"),
            @("renovaciones registradas en la app", "SELECT COUNT(*) FROM (SELECT UserId, RenewalDate FROM cur.Renewals EXCEPT SELECT UserId, RenewalDate FROM main.Renewals);"),
            @("renovaciones con importe", "SELECT COUNT(*) FROM cur.Renewals WHERE RenewalAmount IS NOT NULL;"),
            @("recibos con detalle de clases", "SELECT COUNT(*) FROM cur.RenewalClasses;"),
            @("entradas de lista de espera de la app", "SELECT COUNT(*) FROM (SELECT UserId, ActivityId FROM cur.Waitlists EXCEPT SELECT UserId, ActivityId FROM main.Waitlists);")
        )
        try {
            $counts = Get-SqliteScalars $newDb ($diff | ForEach-Object { $_[1] }) @($attach)
        } catch {
            throw "No se ha podido comparar con la BD actual (puede ser de una version antigua de la app). Abrela una vez con la app para actualizar su esquema, o usa -Force.`n$($_.Exception.Message)"
        }
        for ($i = 0; $i -lt $diff.Count; $i++) {
            if ([int]$counts[$i] -gt 0) { $lost += "$($diff[$i][0]): $($counts[$i])" }
        }

        # Configuracion hecha en la app que no existe en el .mdb
        Invoke-Sqlite $newDb @(
            $attach,
            "INSERT OR REPLACE INTO main.AppSettings (Id, RenewalPeriodMonths, RenewalCost, ClassCost) SELECT Id, RenewalPeriodMonths, RenewalCost, ClassCost FROM cur.AppSettings;",
            "UPDATE main.Activities SET MaxStudents = (SELECT c.MaxStudents FROM cur.Activities c WHERE c.Id = main.Activities.Id) WHERE Id IN (SELECT Id FROM cur.Activities WHERE MaxStudents > 0);"
        ) | Out-Null
        $kept = Get-SqliteScalars $newDb @(
            "SELECT 'periodo ' || RenewalPeriodMonths || ' meses, renovacion ' || RenewalCost || ', clase ' || ClassCost FROM AppSettings WHERE Id = 1;",
            "SELECT COUNT(*) FROM Activities WHERE MaxStudents > 0;"
        )
        Write-Host "   Configuracion conservada: $($kept[0]); clases con plazas maximas: $($kept[1])"
    }

    # -----------------------------------------------------------------------------------------
    # 5. Informe
    # -----------------------------------------------------------------------------------------
    New-Item -ItemType Directory -Force -Path $DataDir | Out-Null
    $report = New-Object System.Collections.Generic.List[string]
    $report.AddRange([string[]]$result.Report)
    $report.Add("")
    $report.Add("=== CAMBIOS DE LA BD ACTUAL QUE SE PIERDEN AL REIMPORTAR: $($lost.Count) ===")
    $lost | ForEach-Object { $report.Add("    $_") }
    [IO.File]::WriteAllLines($reportPath, $report, (New-Object System.Text.UTF8Encoding($true)))
    Write-Host "   Informe: $reportPath"
    if ($result.WaitlistUnmatched -gt 0) {
        Write-Host "   $($result.WaitlistUnmatched) personas en espera no tienen clase asignable: revisa el informe." -ForegroundColor Yellow
    }

    if ($lost.Count -gt 0) {
        Write-Host "La BD actual tiene cambios hechos desde la app que la reimportacion perderia:" -ForegroundColor Yellow
        $lost | ForEach-Object { Write-Host "   - $_" -ForegroundColor Yellow }
        if (-not $Force -and -not $DryRun) {
            Write-Host "No se ha cambiado nada. Usa -Force para reimportar igualmente." -ForegroundColor Red
            return 2
        }
    }

    if ($DryRun) {
        Write-Host "Simulacion: no se ha cambiado nada. BD generada en $newDb" -ForegroundColor Green
        return 0
    }

    # -----------------------------------------------------------------------------------------
    # 6. Copia de seguridad y sustitucion
    # -----------------------------------------------------------------------------------------
    if (Get-Process -Name $AppProcessName -ErrorAction SilentlyContinue) { throw "La aplicacion CONEX se ha abierto durante la reimportacion. Cierrala y vuelve a empezar." }
    if (Test-FileLocked $targetDb) { throw "La BD esta en uso por otro programa: $targetDb" }

    if ($currentExists) {
        New-Item -ItemType Directory -Force -Path $backupDir | Out-Null
        # El nombre no sigue el patron conex_app_yyyyMMdd.db, asi que la app no la borra al rotar sus copias
        $backup = Join-Path $backupDir "conex_app_antes_de_reimportar_$stamp.db"
        Write-Step "Copia de seguridad: $backup"
        # .backup copia tambien lo que haya pendiente en el -wal
        Invoke-Sqlite $targetDb @(".backup '$(ConvertTo-SqlitePath $backup)'") | Out-Null
        if (@(Get-SqliteScalars $backup @("PRAGMA integrity_check;"))[0] -ne "ok") { throw "La copia de seguridad no es valida: $backup" }
    }

    Write-Step "Sustituyendo la BD de la app"
    # Un -wal/-shm antiguo junto a la BD nueva la corromperia: su contenido ya esta en la copia
    foreach ($suffix in @("-wal", "-shm", "-journal")) {
        if (Test-Path -LiteralPath ($targetDb + $suffix)) { Remove-Item -LiteralPath ($targetDb + $suffix) -Force }
    }
    $staging = "$targetDb.reimport_tmp"
    Copy-Item -LiteralPath $newDb -Destination $staging -Force
    if ($currentExists) { [IO.File]::Replace($staging, $targetDb, [NullString]::Value) } else { Move-Item -LiteralPath $staging -Destination $targetDb }

    $final = Get-SqliteScalars $targetDb @("PRAGMA integrity_check;", "SELECT COUNT(*) FROM Waitlists;", "SELECT COUNT(*) FROM Activities WHERE Id IN ($wlIds);")
    if ($final[0] -ne "ok" -or $final[1] -ne "$($result.Waitlist)" -or $final[2] -ne "0") { throw "La BD sustituida no supera la validacion final: $($final -join ' | '). Restaura la copia de seguridad." }
    Write-Host "BD reimportada: $targetDb" -ForegroundColor Green
    return 0
}

$exitCode = 1
try {
    # Lo ultimo que emite la funcion es su codigo de salida
    $exitCode = [int](Invoke-Reimport | Select-Object -Last 1)
}
catch {
    $keepWorkDir = $true
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Los ficheros intermedios quedan en $workDir" -ForegroundColor Red
    $exitCode = 1
}
finally {
    if (-not $keepWorkDir -and (Test-Path -LiteralPath $workDir)) { Remove-Item -LiteralPath $workDir -Recurse -Force }
}
exit $exitCode
