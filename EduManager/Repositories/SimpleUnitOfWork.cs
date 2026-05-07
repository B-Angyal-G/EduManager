using EduManager.Data;
using EduManager.Entities;

namespace EduManager.Repository;

public class SimpleUnitOfWork : IUnitOfWork
{
    private EduDbContext _context;
    private IRepository<User> _userRepository;
    private IRepository<Subject> _subjectRepository;
    private IRepository<Course> _courseRepository;
    private IRepository<CourseSchedule> _courseScheduleRepository;
    private IRepository<NotificationLog> _notificationRepository;
    private IRepository<Grade> _gradeRepository;
    private IRepository<Signature> _signatureRepository;

    public SimpleUnitOfWork(EduDbContext context)
    {
        _context = context;
        _userRepository = new GenericRepository<User>(context);
        _subjectRepository = new GenericRepository<Subject>(context);
        _courseRepository = new GenericRepository<Course>(context);
        _courseScheduleRepository = new GenericRepository<CourseSchedule>(context);
        _notificationRepository = new GenericRepository<NotificationLog>(context);
        _gradeRepository = new GenericRepository<Grade>(context);
        _signatureRepository = new GenericRepository<Signature>(context);
    }

    public IRepository<User> UserRepository => _userRepository;
    public IRepository<Subject> SubjectRepository => _subjectRepository;
    public IRepository<Course> CourseRepository => _courseRepository;
    public IRepository<CourseSchedule> CourseScheduleRepository => _courseScheduleRepository;
    public IRepository<NotificationLog> NotificationRepository => _notificationRepository;
    public IRepository<Grade> GradeRepository => _gradeRepository;
    public IRepository<Signature> SignatureRepository => _signatureRepository;
    
    public async Task SaveAsync() => await _context.SaveChangesAsync();
    
    public void Dispose() => _context.Dispose();
}