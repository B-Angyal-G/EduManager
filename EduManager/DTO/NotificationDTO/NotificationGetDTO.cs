namespace EduManager.DTO.NotificationDTO;

public class NotificationGetDTO
{
    public int Id { get; set; }
    
    // Felhasználó adatai
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;

    // Kurzus adatai
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;

    public DateTime ClassStartTime { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}