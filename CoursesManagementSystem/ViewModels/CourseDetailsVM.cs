using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.ViewModels
{
    public class CourseDetailsVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Details are required")]
        [StringLength(1000, ErrorMessage = "Details cannot exceed 1000 characters")]
        [DataType(DataType.MultilineText)]
        public string Details { get; set; }


        [Url(ErrorMessage = "Invalid URL format")]
        [Required(ErrorMessage = "Book URL is required")]
        [DataType(DataType.Url)]
        public string BookStorageURL { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]

        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Level is required")]
        [Display(Name = "Level")]
        public int LevelId { get; set; }


        //needed for the select loading

        public string CategoryName { get; set; }

        public string LevelName { get; set; }
        //
        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        public string LastModifiedBy { get; set; }
    }
}
