using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoursesManagementSystem.Models
{
    public class Answer : SharedModel
    {
        [Required(ErrorMessage = "Answer text is required")]
         public string AnswerText { get; set; }

        public bool IsCorrect { get; set; }

        [ForeignKey(nameof(Question))]
        [Required(ErrorMessage = "Question ID is required")]
        public int QuestionId { get; set; }
        //Navigation Properties

        public Question? Question { get; set; }
    }
}
