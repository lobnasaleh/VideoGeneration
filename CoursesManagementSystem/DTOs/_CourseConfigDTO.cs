namespace CoursesManagementSystem.DTOs
{
    public class _CourseConfigDTO
    {
        public int Id { get; set; }
        public int ChaptersCount { get; set; }
        public int LessonsCountPerChapter { get; set; }
        public int VideoDurationInMin { get; set; }
        public string Language { get; set; }
        public string Persona { get; set; }
    }
}
