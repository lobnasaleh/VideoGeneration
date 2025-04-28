namespace CoursesManagementSystem.DTOs
{
    public class ChapterCreateDTO
    {
        public string Name { get; set; }
        public string Details { get; set; }
        public int Sort { get; set; }
        public List<LessonCreateDTO> Lessons { get; set; }
    }
}
