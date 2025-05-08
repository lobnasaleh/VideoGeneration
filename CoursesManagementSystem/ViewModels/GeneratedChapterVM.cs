using CoursesManagementSystem.DTOs;

namespace CoursesManagementSystem.ViewModels
{
    public class GeneratedChapterVM
    {
        public int ChapterId { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public int Sort { get; set; }
        public List<GeneratedLessonVM> Lessons { get; set; }

        public int LessonsCount { get; set; }
    }
}
