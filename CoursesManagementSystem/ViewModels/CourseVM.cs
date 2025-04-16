using CoursesManagementSystem.DB.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CoursesManagementSystem.Validations;
using CoursesManagementSystem.Constants;

namespace CoursesManagementSystem.ViewModels
{
    public class CourseVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }


        [Required(ErrorMessage = "Details are required")]
        [StringLength(1000, ErrorMessage = "Details cannot exceed 1000 characters")]
        [DataType(DataType.MultilineText)]
        public string Details { get; set; }


        //[Url(ErrorMessage = "Invalid URL format")]
        [Required(ErrorMessage = "Book is required")]
        /*[AllowedExtensions(UploadsSettings.AllowedExtensions),
         MaxFileSize(UploadsSettings.MaxFileSizeInBytes) ]*/
        public IFormFile Book { get; set; }

        public string BookStorageURL { get; set; }//3shan el edit a3raf abayen feeha esm el file ely ma3mlo upload




        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]

        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Level is required")]
        [Display(Name="Level")]
        public int LevelId { get; set; }


        //needed for the select loading

        public string CategoryName { get; set; }

        public string LevelName { get; set; }

        //to show them in selects
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();//
        public IEnumerable<Level> Levels { get; set; } =new List<Level>();


    }
}
