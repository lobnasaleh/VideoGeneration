using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.DTOs
{
    public class ChapterDTO
    {
        public int ChapterId { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public int Sort { get; set; }
        public List<LessonDTO> Lessons { get; set; }
    }
}

