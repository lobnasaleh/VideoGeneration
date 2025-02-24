using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.DB.Models
{
    public class Category : SharedModel
    {
        [Required(ErrorMessage = "Category Name is Required")]
        [StringLength(50, ErrorMessage = "Category Name cannot exceed 50 characters")]
        public string Name { get; set; }
        public ICollection<Course> Courses { get; set; }
    }
}
