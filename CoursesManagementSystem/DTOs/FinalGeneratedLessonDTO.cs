namespace CoursesManagementSystem.DTOs
{
    public class FinalGeneratedLessonDTO
    {
        public string Name{ get; set; }
        public string Details { get; set; }
        public string ScriptText { get; set; }
        public string VideoStorageURL { get; set; }
        public List<FinalGeneratedQuestionDTO> Questions { get; set; }
    }
}
