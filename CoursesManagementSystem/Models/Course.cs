using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoursesManagementSystem.Models
{
    public class Course : SharedModel
    {
        [Required(ErrorMessage = "Course name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Details are required")]
        [StringLength(1000, ErrorMessage = "Details cannot exceed 1000 characters")]
        public string Details { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        [Required(ErrorMessage = "Book URL is required")]
        public string BookStorageURL { get; set; }

        [ForeignKey(nameof(Category))]
        [Required(ErrorMessage = "Category ID is required")]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(Level))]
        [Required(ErrorMessage = "Level ID is required")]
        public int LevelId { get; set; }

        //Navigation Properties 

        public virtual Category? Category { get; set; }

        public virtual Level? Level { get; set; }

        public virtual CourseConfig? CourseConfig { get; set; }

        public virtual ICollection<CourseQuestionConfig>? CourseQuestionsConfig { get; set; }
    }
}
