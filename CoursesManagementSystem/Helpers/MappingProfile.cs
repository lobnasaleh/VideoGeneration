using AutoMapper;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Enums;
using CoursesManagementSystem.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CoursesManagementSystem.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile() {

           CreateMap<Level,LevelVM>().ReverseMap();
            CreateMap<Level, LevelDetailsVM>().ReverseMap();

            CreateMap<Course, CourseVM>().ReverseMap()
                 .ForMember(dest => dest.Category, opt => opt.Ignore())
                 .ForMember(dest => dest.Level, opt => opt.Ignore());

            CreateMap<Course, UpdateCourseVM>()
                .ReverseMap()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Level, opt => opt.Ignore());

            
            CreateMap<Chapter, ChapterVM>().ReverseMap()
               .ForMember(dest => dest.Course, opt => opt.Ignore());

            CreateMap<Lesson, LessonVM>().ReverseMap()
              .ForMember(dest => dest.Chapter, opt => opt.Ignore());

            CreateMap<Lesson, UpdateLessonVM>()
            .ReverseMap()
         .ForMember(dest => dest.Chapter, opt => opt.Ignore());


        }
    }
}
