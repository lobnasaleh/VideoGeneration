using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.DTOs
{
    public class CourseConfigDTO
    {
        public int ChaptersCount { get; set; }

        public int LessonsCountPerChapter { get; set; }

        public int VideoDurationInMin { get; set; }

        public string Language { get; set; }
        public string Persona { get; set; }

        public int CourseId { get; set; }
        public string BookStorageURL { get; set; }
        public string CourseName { get; set; }

    }
}
