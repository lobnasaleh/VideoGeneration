using CoursesManagementSystem.DB.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CoursesManagementSystem.Enums;

namespace CoursesManagementSystem.DTOs
{
    public class CoursesDetailsDTO
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public string BookStorageURL { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public int ChaptersCount { get; set; }
        public int LessonsCountPerChapter { get; set; }
        public int VideoDurationInMin { get; set; }
        public string Language { get; set; }
        public string Persona { get; set; }
        public List<CourseQuestionConfigDTO> CourseQuestionConfig {  get; set; }
        public List<ChapterDTO> Chapters { get; set; }


    }
}
