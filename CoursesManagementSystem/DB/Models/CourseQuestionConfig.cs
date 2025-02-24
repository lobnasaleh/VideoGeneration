using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoursesManagementSystem.DB.Models
{
    public class CourseQuestionConfig : SharedModel
    {
        public int QuestionsCountPerLesson { get; set; }
        [ForeignKey(nameof(Course))]
        [Required(ErrorMessage = "Course ID is required.")]
        public int CourseId { get; set; }//1
        [ForeignKey(nameof(QuestionLevel))]
        [Required(ErrorMessage = "Question Level ID is required.")]
        public int QuestionLevelId { get; set; }//1-->easy
        // Navigation Properties
        public  Course Course { get; set; }
        public QuestionLevel QuestionLevel { get; set; }


    }
}
