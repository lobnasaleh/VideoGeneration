using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.ViewModels
{
    public class LevelVM
    {
        [Required(ErrorMessage = "Course Level Name is Required")]
        public string Name { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Sort order must be at least 1")]
        public int Sort { get; set; }
    }
}
