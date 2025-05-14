using CoursesManagementSystem.DTOs;

namespace CoursesManagementSystem.ViewModels
{
    public class GeneratedCourseVM
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public string BookStorageURL { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public int ChaptersCount { get; set; }
        public int LessonsCountPerChapter { get; set; }
        public int VideoDurationInMin { get; set; }
        public int FirstGeneratedLessonOfChapterOneID { get; set; }
        public int TotalCourseDuration { get; set; }
        public string Language { get; set; }
        public string CourseImage { get; set; }
        public string Persona { get; set; }
        public List<GeneratedCourseQuestionConfigVM> CourseQuestionConfig { get; set; }
        public List<GeneratedChapterVM> Chapters { get; set; }





    }
}
