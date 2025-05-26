using CoursesManagementSystem.DB.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.ViewModels
{
    public class ChapterVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Chapter name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Details are required")]
        public string Details { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Chapter order must be at least 1")]
        [Display(Name = "Chapter Order")]
        public int? Sort { get; set; }

        [ForeignKey(nameof(Course))]
        [Required(ErrorMessage = "Course is required")]
        [Display (Name="Course")]
        public int CourseId { get; set; }

        public string CourseName { get; set; } //for select loading
     


    }
}
