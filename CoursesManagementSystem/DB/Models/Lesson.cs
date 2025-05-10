using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoursesManagementSystem.DB.Models
{
    public class Lesson : SharedModel
    {
        [Required(ErrorMessage = "Lesson name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Details are required")]
        [StringLength(1000, ErrorMessage = "Details cannot exceed 1000 characters")]
        public string Details { get; set; }


        [Required(ErrorMessage = "Scriptis required")]
        [DataType(DataType.MultilineText)]
        public string ScriptText { get; set; }


        [Url(ErrorMessage = "Invalid URL format")]
        [Required(ErrorMessage = "Video URL is required")]
        public string VideoStorageURL { get; set; }


        [Url(ErrorMessage = "Invalid URL format")]
        [Required(ErrorMessage = "Audio URL is required")]
        public string AudioStorageURL { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Sort order must be at least 1")]
        public int Sort { get; set; }


        [ForeignKey(nameof(Chapter))]
        [Required(ErrorMessage = "Chapter ID is required")]
        public int ChapterId { get; set; }

        //Navigation Proprties

        public Chapter Chapter { get; set; }

        public ICollection<Question> Questions { get; set; }


    }
}
