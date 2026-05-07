namespace EduManager.Entities;

public class Signature
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public int StudentId { get; set; }
    public User? Student { get; set; }
    public string Semester { get; set; } = string.Empty;
    public bool IsSigned { get; set; } // true = Aláírva, false = Megtagadva
    public DateTime Time { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}