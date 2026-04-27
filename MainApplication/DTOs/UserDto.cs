namespace CONEX_APP.MainApplication.DTOs;

public class UserDto
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;
    
    public string SecondSurname { get; set; } = string.Empty;

    public string IdCard { get; set; } = string.Empty;
    
    public string Address { get; set; } = string.Empty;
    
    public string Location { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    
    public bool IsPartner {  get; set; }

    public bool IsTutor { get; set; }

    public string FullName => $"{Name} {Surname}";
    
    public DateTime CreatedAt { get; set; }

    public DateTime DischargedAt { get; set; }
}