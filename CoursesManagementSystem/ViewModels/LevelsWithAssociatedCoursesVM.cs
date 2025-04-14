using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.ViewModels
{
    public class LevelsWithAssociatedCoursesVM
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Course Level Name is Required")]
        public string Name { get; set; }
        [Range(1, 10, ErrorMessage = "Difficulty Level must be between 1 and 10")]
        [Required(ErrorMessage = "Difficulty Number is Required")]
        [Display(Name = "Difficulty Level")]
        public int Sort { get; set; }

        public int AssociatedCourses { get; set; } = 0;
    }
}
