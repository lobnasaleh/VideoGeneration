using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.ViewModels
{
    public class LessonDetailsVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lesson name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Details are required")]
        [StringLength(1000, ErrorMessage = "Details cannot exceed 1000 characters")]
        [DataType(DataType.MultilineText)]
        public string Details { get; set; }
        [Required(ErrorMessage = "Script is required")]
        [DataType(DataType.MultilineText)]
        public string ScriptText { get; set; }
        [Url(ErrorMessage = "Invalid URL format")]
        [Required(ErrorMessage = "Video URL is required")]
        public string VideoStorageURL { get; set; }
        [Url(ErrorMessage = "Invalid URL format")]
        [Required(ErrorMessage = "Audio URL is required")]
        public string AudioStorageURL { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Sort order must be at least 1")]
        [Display(Name = "Lesson Order")]
        public int Sort { get; set; }
        [Required(ErrorMessage = "Chapter is required")]
        [Display(Name = "Chapter")]
        public int ChapterId { get; set; }

        //for select loading
        public string ChapterName { get; set; }
        public string CourseName { get; set; }

        //
        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        public string LastModifiedBy { get; set; }
    }
}
