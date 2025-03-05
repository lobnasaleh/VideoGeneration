using CoursesManagementSystem.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoursesManagementSystem.DB.Models
{
    public class Question : SharedModel
    {
        [Required(ErrorMessage = "Question text is required")]
        public string QuestionText { get; set; }


        [Required(ErrorMessage = "Question Instructions are required")]
        public string QuestionInstructions { get; set; }


        [Required(ErrorMessage = "Question type is required")]
        public QuestionTypeEnum QuestionType { get; set; }


        [ForeignKey(nameof(Lesson))]
        [Required(ErrorMessage = "Lesson ID is required")]
        public int LessonId { get; set; }


        [ForeignKey(nameof(QuestionLevel))]
        [Required(ErrorMessage = "Question level ID is required")]
        public int QuestionLevelId { get; set; }

        //Navigation Properties

        public QuestionLevel QuestionLevel { get; set; }

        public  Lesson Lesson { get; set; }

        public  ICollection<Answer> Answers { get; set; }



    }
}
