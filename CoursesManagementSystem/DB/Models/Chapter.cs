using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoursesManagementSystem.DB.Models
{
    public class Chapter : SharedModel
    {
        [Required(ErrorMessage = "Chapter name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Details are required")]
        [DataType(DataType.MultilineText)]
        public string Details { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Sort order must be at least 1")]
        public int? Sort { get; set; }

        [ForeignKey(nameof(Course))]
        [Required(ErrorMessage = "Course ID is required")]
        public int CourseId { get; set; }

        //Navigation Properties 

        public  Course Course { get; set; }

        public ICollection<Lesson> Lessons { get; set; }


    }
}
