namespace CoursesManagementSystem.ViewModels
{
    public class LessonDetailViewModel
    {
        public int LessonId { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public string ScriptText { get; set; }
        public string VideoStorageURL { get; set; }
        public string ChapterName { get; set; }
        public int? NextLessonId { get; set; }

        public List<QuestionViewModel> Questions { get; set; }
    }
}
