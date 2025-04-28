namespace CoursesManagementSystem.DTOs
{
    public class CourseQuestionConfigDTO
    {
        public int CourseId { get; set; } 
        public int QuestionsCountPerLesson { get; set; }
        public int QuestionLevelId { get; set; }
        public string QuestionLevelName { get; set; }
    }
}
