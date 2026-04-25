using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduManager.Repository;
using EduManager.DTO.NotificationDTO;
using AutoMapper;

namespace EduManager.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationGetDTO>>> GetNotifications(int? userId, int? courseId)
        {
            var includes = new[] { "User", "Course" };

            var logs = await _unitOfWork.NotificationRepository.GetAllAsync(n => 
                    (!userId.HasValue || n.UserId == userId) && 
                    (!courseId.HasValue || n.CourseId == courseId),
                includes);

            return Ok(_mapper.Map<List<NotificationGetDTO>>(logs));
        }
    }
}
