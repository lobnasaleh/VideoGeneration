namespace CoursesManagementSystem.DTOs
{
    public class CourseGenerationDTO
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public string BookStorageURL { get; set; }
        public int ChaptersCount { get; set; }
        public int LessonsCountPerChapter { get; set; }
        public int VideoDurationInMin { get; set; }
        public string Language { get; set; }
        public string Persona { get; set; }
        public List<CourseQuestionConfigToAiDTO> CourseQuestionConfig { get; set; }
    }
}
