using EduManager.Entities;

namespace EduManager.Repository;

public interface IUnitOfWork
{
    IRepository<User> UserRepository { get; }
    IRepository<Subject> SubjectRepository { get; }
    IRepository<Course> CourseRepository { get; }
    IRepository<CourseSchedule> CourseScheduleRepository { get; }
    IRepository<NotificationLog> NotificationRepository { get; }
    Task SaveAsync();
}