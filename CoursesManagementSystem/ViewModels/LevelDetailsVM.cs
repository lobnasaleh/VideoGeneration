using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.ViewModels
{
    public class LevelDetailsVM
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Course Level Name is Required")]
        public string Name { get; set; }
        [Range(1, 10, ErrorMessage = "Difficulty Level must be between 1 and 10")]
        [Required(ErrorMessage = "Difficulty Number is Required")]
        [Display(Name = "Difficulty Level")]
        public int Sort { get; set; }
        //
        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        public string LastModifiedBy { get; set; }
    }
}
