using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.Marshalling;

namespace CoursesManagementSystem.Models
{
    public class QuestionLevel : SharedModel
    {
        [Required(ErrorMessage = "Level name is required")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; }
        [Required(ErrorMessage = "DifficultyScore is required")]

        [Range(1, 100, ErrorMessage = "Difficulty score must be between 1 and 100")]
        public int DifficultyScore { get; set; }    

        //Navigation Properties

        public virtual ICollection<Question>?Questions { get; set; }

        public virtual CourseQuestionConfig CourseQuestionConfig { get; set; }

    }
}
