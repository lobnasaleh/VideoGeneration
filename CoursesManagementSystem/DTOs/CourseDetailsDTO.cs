namespace CoursesManagementSystem.DTOs
{
    public class CourseDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public string BookStorageURL { get; set; }

        public string CategoryName { get; set; }
        public string LevelName { get; set; }

        public _CourseConfigDTO CourseConfig { get; set; }

        public List<_CourseQuestionConfigDTO> CourseQuestionsConfig { get; set; }
    }
}
