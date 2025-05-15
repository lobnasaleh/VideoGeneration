using CoursesManagementSystem.Constants;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Validations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.ViewModels
{
    public class UpdateCourseVM
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Course name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Details are required")]
        [StringLength(1000, ErrorMessage = "Details cannot exceed 1000 characters")]
        [DataType(DataType.MultilineText)]
        public string Details { get; set; }
  // [AllowedExtensions(UploadsSettings.AllowedExtensions),
    //     MaxFileSize(UploadsSettings.MaxFileSizeInBytes) ]
        public IFormFile? Book { get; set; }

        public string BookStorageURL { get; set; }//3shan el edit a3raf abayen feeha esm el file ely ma3mlo upload

        public IFormFile CourseImage { get; set; }

        public string CourseImageStorageURL { get; set; }//3shan el edit a3raf abayen feeha esm el Image ely ma3mlo upload

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]

        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Level is required")]
        [Display(Name = "Level")]
        public int LevelId { get; set; }


        //needed for the select loading

        public string CategoryName { get; set; }

        public string LevelName { get; set; }

        //to show them in selects
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Level> Levels { get; set; }
    }
}
