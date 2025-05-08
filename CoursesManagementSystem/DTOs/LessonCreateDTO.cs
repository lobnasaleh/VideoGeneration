namespace CoursesManagementSystem.DTOs
{
    public class LessonCreateDTO
    {
        public int LessonId { get; set; }
        public string Details { get; set; }
        public string ScriptText { get; set; }
        public string VideoStorageURL { get; set; }
        public string AudioStorageURL { get; set; }
        public List<QuestionCreateDTO> Questions { get; set; }
    }
}
