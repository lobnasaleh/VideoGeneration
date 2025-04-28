namespace CoursesManagementSystem.DTOs
{
    public class PhaseOneCreateDTO
    {
        public int CourseId { get; set; }
        public List<ChapterCreateDTO> Chapters { get; set; }
    }
}
