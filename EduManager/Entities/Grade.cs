namespace EduManager.Entities;

public class Grade
{
    public int Id { get; set; }
    
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public int StudentId { get; set; }
    public User? Student { get; set; }

    public string Semester { get; set; } = string.Empty;
    
    public int GradeValue { get; set; }
    
    public DateTime Time { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}