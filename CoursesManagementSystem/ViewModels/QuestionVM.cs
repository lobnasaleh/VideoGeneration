using CoursesManagementSystem.Enums;
using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.ViewModels
{
    public class QuestionVM
    {
        public int ID { get; set; } // Represents the Question ID

        [Required(ErrorMessage = "Question text is required")]
        public string QuestionText { get; set; }


        [Required(ErrorMessage = "Question Instructions are required")]
        public string QuestionInstructions { get; set; }


        [Required(ErrorMessage = "Question type is required")]
        [Display(Name = "Question Type")]
        public QuestionTypeEnum QuestionType { get; set; }

        [Required(ErrorMessage = "Lesson is required")]
        public int LessonId { get; set; }

        [Required(ErrorMessage = "Question level is required")]
        public int QuestionLevelId { get; set; }

        // Additional properties to display related names in views
        public string LessonName { get; set; }
        public string QuestionLevelName { get; set; }

        public string CourseName { get; set; }
    }
}
