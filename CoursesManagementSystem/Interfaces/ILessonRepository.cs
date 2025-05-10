using CoursesManagementSystem.DB.Models;

namespace CoursesManagementSystem.Interfaces
{
    public interface ILessonRepository:IBaseRepository<Lesson>
    {
        Task<Lesson> GetLessonWithQuestionsAndAnswersAsync(int lessonId);
    }
}
