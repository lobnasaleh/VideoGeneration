namespace CoursesManagementSystem.ViewModels
{
    public class QuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionInstructions { get; set; }
        public string QuestionType { get; set; }

        public List<AnswerViewModel> Answers { get; set; }
    }
}
