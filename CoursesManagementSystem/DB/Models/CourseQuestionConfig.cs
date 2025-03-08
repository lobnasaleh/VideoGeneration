using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoursesManagementSystem.DB.Models
{
    public class CourseQuestionConfig : SharedModel
    {
        [Display(Name = "Questions Count Per Lesson")]

        public int QuestionsCountPerLesson { get; set; }


        [ForeignKey(nameof(Course))]
        [Required(ErrorMessage = "Course is required.")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }//1

      
        [ForeignKey(nameof(QuestionLevel))]
        [Required(ErrorMessage = "Question Level is required.")]
        [Display(Name = "Question Level")]
        public int QuestionLevelId { get; set; }//1-->easy
        // Navigation Properties
        public  Course Course { get; set; }
        public QuestionLevel QuestionLevel { get; set; }


    }
}
