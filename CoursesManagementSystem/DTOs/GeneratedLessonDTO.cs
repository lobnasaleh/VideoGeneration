using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.DTOs
{
    public class GeneratedLessonDTO
    {
        public int LessonId { get; set; }
        public string ScriptText { get; set; }
        public string VideoStorageURL { get; set; }
        public string AudioStorageURL { get; set; }
        public List<QuestionCreateDTO> Questions { get; set; }
    }
}
