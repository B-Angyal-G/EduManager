namespace EduManager.Entities;

public class NotificationLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public DateTime ClassStartTime { get; set; } // Melyik óra miatt jött az értesítés
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}