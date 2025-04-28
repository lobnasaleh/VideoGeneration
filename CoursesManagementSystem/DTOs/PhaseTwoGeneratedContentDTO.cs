namespace CoursesManagementSystem.DTOs
{
    public class PhaseTwoGeneratedContentDTO
    {
        public int CourseId { get; set; }

        public List<GeneratedChapterDTO> Chapters {  get; set; }
    }
}
