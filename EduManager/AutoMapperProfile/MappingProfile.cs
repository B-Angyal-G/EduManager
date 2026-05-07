using AutoMapper;
using EduManager.Entities;
using EduManager.DTO.UserDto;
using EduManager.DTO.SubjectDTO;
using EduManager.DTO.CourseDTO;
using EduManager.DTO.NotificationDTO;
using EduManager.DTO.GradeDTO;
using EduManager.DTO.SignatureDTO;

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
        CreateMap<SubjectUpdateDTO, Subject>();

        // COURSE leképezések
        CreateMap<Course, CourseGetDTO>()
            .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject != null ? src.Subject.Name : "N/A"))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Form, opt => opt.MapFrom(src => src.Form.ToString()))
            .ForMember(dest => dest.HoursDescription, opt => opt.MapFrom(src => 
                $"{src.Hours} {(src.HourUnit == HourType.Weekly ? "heti" : "féléves")}"))
            .ForMember(dest => dest.TeacherNames, opt => opt.MapFrom(src => src.Teachers.Select(t => t.Username).ToList()));
        CreateMap<CourseCreateDTO, Course>();
        CreateMap<CourseUpdateDTO, Course>();
        
        // NOTIFICATION leképezések
        CreateMap<NotificationLog, NotificationGetDTO>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : "Ismeretlen"))
            .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Course != null ? src.Course.CourseCode : "N/A"));
        
        // GRADE leképezések
        CreateMap<Grade, GradeGetDTO>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student != null ? src.Student.Username : "Ismeretlen"))
            .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject != null ? src.Subject.Name : "Ismeretlen"));
        
        // Aláírás lekérés mappolása (nevekkel)
        CreateMap<Signature, SignatureGetDTO>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => 
                src.Student != null ? src.Student.Username : "Ismeretlen"))
            .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => 
                src.Subject != null ? src.Subject.Name : "Ismeretlen"));

        // Aláírás létrehozás mappolása (DTO -> Entity)
        CreateMap<SignatureCreateDTO, Signature>()
            .ForMember(dest => dest.IsSigned, opt => opt.MapFrom(src => src.Value));
    }
}