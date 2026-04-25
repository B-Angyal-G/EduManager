using AutoMapper;
using EduManager.Entities;
using EduManager.DTO.UserDto;
using EduManager.DTO.SubjectDTO;
using EduManager.DTO.CourseDTO;
using EduManager.DTO.NotificationDTO;

namespace EduManager.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // USER leképezések
        CreateMap<User, UserGetDTO>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.StudyMode, opt => opt.MapFrom(src => 
                src.StudyMode == StudyMode.None ? "N/A" : src.StudyMode.ToString()));

        CreateMap<UserCreateDTO, User>();
        CreateMap<UserUpdateDTO, User>();

        // SUBJECT leképezések
        CreateMap<Subject, SubjectGetDTO>();
        CreateMap<SubjectCreateDTO, Subject>();

        // COURSE leképezések
        CreateMap<Course, CourseGetDTO>()
            .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject != null ? src.Subject.Name : "N/A"))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Form, opt => opt.MapFrom(src => src.Form.ToString()))
            .ForMember(dest => dest.HoursDescription, opt => opt.MapFrom(src => 
                $"{src.Hours} {(src.HourUnit == HourType.Weekly ? "heti" : "féléves")}"))
            .ForMember(dest => dest.TeacherNames, opt => opt.MapFrom(src => src.Teachers.Select(t => t.Username).ToList()));

        CreateMap<CourseCreateDTO, Course>();
        
        // NOTIFICATION leképezések
        CreateMap<NotificationLog, NotificationGetDTO>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : "Ismeretlen"))
            .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Course != null ? src.Course.CourseCode : "N/A"));
    }
}