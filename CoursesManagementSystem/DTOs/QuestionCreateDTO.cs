using CoursesManagementSystem.Enums;

namespace CoursesManagementSystem.DTOs
{
    public class QuestionCreateDTO
    {
        public string QuestionText { get; set; }
        public string QuestionInstructions { get; set; }
        public QuestionTypeEnum QuestionType { get; set; }
        public int QuestionLevelId { get; set; }
        public List<AnswerCreateDTO> Answers { get; set; }

    }

    
}
