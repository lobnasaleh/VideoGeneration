namespace CoursesManagementSystem.DTOs
{
    public class ChapterCreateDTO
    {
        public int ChapterId { get; set; }
        public string Name { get; set; } 
        public string Details { get; set; } 
        public List<LessonCreateDTO> Lessons { get; set; }
    }
}
