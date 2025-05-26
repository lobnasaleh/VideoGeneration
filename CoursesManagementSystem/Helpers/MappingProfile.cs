using AutoMapper;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Enums;
using CoursesManagementSystem.ViewModels;
using CoursesManagementSystem.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static CoursesManagementSystem.DTOs.CourseDTO;

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


            CreateMap<Course, CourseDTO>()
    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
    .ForMember(dest => dest.LevelName, opt => opt.MapFrom(src => src.Level.Name));


            CreateMap<Chapter, ChapterDTO>();

            CreateMap<Course, CourseDetailsDTO>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.LevelName, opt => opt.MapFrom(src => src.Level.Name))
            .ForMember(dest => dest.CourseConfig, opt => opt.MapFrom(src => src.CourseConfig))
            .ForMember(dest => dest.CourseQuestionsConfig, opt => opt.MapFrom(src => src.CourseQuestionsConfig));

            CreateMap<CourseConfig, _CourseConfigDTO>()
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language.ToString()))
                .ForMember(dest => dest.Persona, opt => opt.MapFrom(src => src.Persona.ToString()));

            CreateMap<CourseQuestionConfig, _CourseQuestionConfigDTO>()
                .ForMember(dest => dest.QuestionLevelName, opt => opt.MapFrom(src => src.QuestionLevel.Name));

            CreateMap<PhaseOneCreateDTO, Chapter>()
            .ForMember(dest => dest.Course, opt => opt.MapFrom(src => src.CourseId));

            CreateMap<LessonCreateDTO, Lesson>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

            CreateMap<QuestionCreateDTO, Question>()
                .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers));

            CreateMap<Chapter, _ChapterDTO>();

            CreateMap<PhaseOneCreateDTO, Course>(); // optional
            CreateMap<ChapterCreateDTO, Chapter>();
            CreateMap<LessonCreateDTO, Lesson>();
            CreateMap<QuestionCreateDTO, Question>();
            CreateMap<AnswerCreateDTO, Answer>();
            CreateMap<CreateFullCourseDTO, Course>();

            CreateMap<Course, CourseGenerationDTO>()
             .ForMember(dest => dest.CourseQuestionConfig,
                opt => opt.MapFrom(src => src.CourseQuestionsConfig));

            CreateMap<CourseQuestionConfig, CourseQuestionConfigToAiDTO>()
                .ForMember(dest => dest.QuestionLevelName,
                    opt => opt.MapFrom(src => src.QuestionLevel.Name));

            CreateMap<Course, CourseGenerationDTO>();
            CreateMap<CourseQuestionConfig, CourseQuestionConfigToAiDTO>()
                .ForMember(dest => dest.QuestionLevelName, opt => opt.MapFrom(src => src.QuestionLevel.Name));




        }
    }
}
