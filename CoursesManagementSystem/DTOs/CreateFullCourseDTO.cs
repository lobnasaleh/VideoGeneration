namespace CoursesManagementSystem.DTOs
{
    public class CreateFullCourseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public string BookStorageURL { get; set; }
        public int CategoryId { get; set; }
        public int LevelId { get; set; }

        public List<ChapterCreateDTO> Chapters { get; set; }
    }
}
