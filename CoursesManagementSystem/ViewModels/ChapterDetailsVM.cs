using CoursesManagementSystem.DB.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.ViewModels
{
    public class ChapterDetailsVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Chapter name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Details are required")]
        public string Details { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Chapter order must be at least 1")]
        [Display(Name = "Chapter Order")]
        public int Sort { get; set; }

        [ForeignKey(nameof(Course))]
        [Required(ErrorMessage = "Course is required")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        public string CourseName { get; set; } //for select loading
        //
        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        public string LastModifiedBy { get; set; }
    }
}
