namespace CoursesManagementSystem.DTOs
{
    public class ChapterCreateDTO
    {
        public int ChapterId { get; set; }
        public List<LessonCreateDTO> Lessons { get; set; }
    }
}
