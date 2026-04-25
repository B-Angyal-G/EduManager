namespace EduManager.Entities;

public class CourseSchedule
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}