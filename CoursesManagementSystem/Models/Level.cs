using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.Models
{
    public class Level : SharedModel
    {
        [Required(ErrorMessage = "Course Level is Required")]
        public string Name { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Sort order must be at least 1")]
        public int Sort {  get; set; }
        public virtual ICollection<Course>? Courses { get; set; }

    }
}
