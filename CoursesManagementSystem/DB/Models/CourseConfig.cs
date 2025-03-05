using CoursesManagementSystem.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoursesManagementSystem.DB.Models
{
    public class CourseConfig : SharedModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Chapters count must be at least 1.")]
        public int ChaptersCount { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Lessons count per chapter must be at least 1.")]
        public int LessonsCountPerChapter { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Video duration must be at least 1 minute.")]
        public int VideoDurationInMin { get; set; }


        [Required(ErrorMessage = "Language is required.")]
        public VideoPresenterLanguageEnum Language { get; set; }


        [Required(ErrorMessage = "Persona is required.")]
        public VideoPresenterPersonaEnum Persona { get; set; }

        [ForeignKey(nameof(Course))]
        [Required(ErrorMessage = "Course ID is required.")]
        public int CourseId { get; set; }


        //Navigation Properties 
        public Course Course { get; set; }

    }
}
