using CoursesManagementSystem.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoursesManagementSystem.Models
{
    public class Question : SharedModel
    {
        [Required(ErrorMessage = "Question text is required")]
        public string QuestionText { get; set; }
        [Required(ErrorMessage = "Question Instructions are required")]

        public string QuestionInstructions { get; set; }
        [Required(ErrorMessage = "Question type is required")]
        public QuestionType QuestionType { get; set; }

        [ForeignKey(nameof(Lesson))]
        [Required(ErrorMessage = "Lesson ID is required")]
        public int LessonId { get; set; }

        [ForeignKey(nameof(QuestionLevel))]
        [Required(ErrorMessage = "Question level ID is required")]
        public int QuestionLevelId { get; set; }

        //Navigation Properties

        public virtual QuestionLevel? QuestionLevel { get; set; }

        public virtual Lesson? Lesson { get; set; }

        public virtual ICollection<Answer>? Answers { get; set; }
         


    }
}
