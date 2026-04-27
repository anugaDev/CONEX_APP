using System;

namespace CONEX_APP.Presentation.Helpers;

public class ActivityDateCalculator
{
    public DateTime GetNextOccurrence(string dayName, string timeString)
    {
        int dayOffset = dayName switch
        {
            "Lunes" => 1,
            "Martes" => 2,
            "Miércoles" => 3,
            "Jueves" => 4,
            "Viernes" => 5,
            "Sábado" => 6,
            "Domingo" => 7,
            _ => 1
        };

        int currentDayOfWeek = (int)DateTime.Today.DayOfWeek;
        if (currentDayOfWeek == 0) currentDayOfWeek = 7; 

        int daysToAdd = dayOffset - currentDayOfWeek;
        if (daysToAdd < 0) daysToAdd += 7; 

        DateTime date = DateTime.Today.AddDays(daysToAdd);
        TimeSpan time = TimeSpan.Parse(timeString);
        
        return date.Add(time);
    }
}
