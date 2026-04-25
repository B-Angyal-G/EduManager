using EduManager.Entities;
using EduManager.Repository;

namespace EduManager;

public class NotificationBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                
                // 30 perc múlva kezdődő órák keresése (29-31 perc közötti ablakban, hogy ne maradjunk le)
                var targetTimeStart = DateTime.Now.AddMinutes(29);
                var targetTimeEnd = DateTime.Now.AddMinutes(31);

                var upcomingClasses = await unitOfWork.CourseScheduleRepository.GetAllAsync(
                    s => s.StartTime >= targetTimeStart && s.StartTime <= targetTimeEnd, 
                    new[] { "Course.Students", "Course.Teachers" });

                foreach (var schedule in upcomingClasses)
                {
                    // Minden oktató és hallgató értesítése
                    var participants = schedule.Course!.Students.Concat(schedule.Course.Teachers);

                    foreach (var person in participants)
                    {
                        // Ellenőrizzük, küldtünk-e már ehhez az órához értesítést (duplikáció elkerülése)
                        var alreadyNotified = (await unitOfWork.NotificationRepository.GetAllAsync(
                            n => n.UserId == person.Id && n.CourseId == schedule.CourseId && n.ClassStartTime == schedule.StartTime)).Any();

                        if (!alreadyNotified)
                        {
                            unitOfWork.NotificationRepository.Add(new NotificationLog
                            {
                                UserId = person.Id,
                                CourseId = schedule.CourseId,
                                ClassStartTime = schedule.StartTime,
                                Message = $"Emlékeztető: A(z) {schedule.Course.CourseCode} kurzus 30 perc múlva kezdődik!"
                            });
                        }
                    }
                }
                await unitOfWork.SaveAsync();
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Percenként ellenőriz
        }
    }
}