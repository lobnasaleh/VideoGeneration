using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.DTOs
{
    public class AnswerCreateDTO
    {
        
            public string AnswerText { get; set; }
            public bool IsCorrect { get; set; }
        
    }
}
