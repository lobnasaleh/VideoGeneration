using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.DTOs
{
    public class LessonDTO
    {
        public int LessonId { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public string ScriptText { get; set; }
        public string VideoStorageURL { get; set; }
        public string? AudioStorageURL { get; set; }
        public int? Sort { get; set; }
    }
}
