using CoursesManagementSystem.Enums;

namespace CoursesManagementSystem.DTOs
{
    public class FinalGeneratedQuestionDTO
    {
        public string QuestionText { get; set; }
        public string QuestionInstructions { get; set; }
        public QuestionTypeEnum QuestionType { get; set; }
        public int QuestionLevelId { get; set; }
        public List<FinalGeneratedAnswerDTO> Answers { get; set; }
    }
}
