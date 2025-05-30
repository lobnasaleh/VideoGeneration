﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoursesManagementSystem.DB.Models
{
    public class Course : SharedModel
    {
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

        public string? CourseImageStorageURL {  get; set; }


        [ForeignKey(nameof(Category))]
        [Required(ErrorMessage = "Category ID is required")]
        public int CategoryId { get; set; }


        [ForeignKey(nameof(Level))]
        [Required(ErrorMessage = "Level ID is required")]
        public int LevelId { get; set; }
      //  public string? UserId { get; set; }

        //Navigation Properties 
       // public ApplicationUser? User { get; set; }

        public Category Category { get; set; }

        public Level Level { get; set; }

        public CourseConfig CourseConfig { get; set; }

        public ICollection<CourseQuestionConfig> CourseQuestionsConfig { get; set; }
    }
}
