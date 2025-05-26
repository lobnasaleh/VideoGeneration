using CoursesManagementSystem.DB.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.ViewModels
{
    public class LessonVM
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

        //  [Url(ErrorMessage = "Invalid URL format")]
        [Required(ErrorMessage = "Video is required")]
        public IFormFile Video { get; set; }
        // [Url(ErrorMessage = "Invalid URL format")]
         [Required(ErrorMessage = "Audio is required")]
        public IFormFile Audio { get; set; }

        public string VideoStorageURL { get; set; }//3shan el edit a3raf abayen feeha esm el file ely ma3mlo upload
        public string AudioStorageURL { get; set; }//3shan el edit a3raf abayen feeha esm el file ely ma3mlo upload

        [Range(1, int.MaxValue, ErrorMessage = "Sort order must be at least 1")]
        [Display(Name = "Lesson Order")]
        public int? Sort { get; set; }
        [Required(ErrorMessage = "Chapter is required")]
        [Display (Name="Chapter")]
        public int ChapterId { get; set; }

        //for select loading
        public string ChapterName { get; set; }

        public string CourseName { get; set; }


    }
}
