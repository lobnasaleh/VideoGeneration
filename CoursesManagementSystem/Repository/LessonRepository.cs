using CoursesManagementSystem.Data;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;

namespace CoursesManagementSystem.Repository
{
    public class LessonRepository:BaseRepository<Lesson>,ILessonRepository
    {
        private readonly ApplicationDbContext _context;
        public LessonRepository(ApplicationDbContext _context) : base(_context)
        {
            this._context = _context;
        }

        public async Task<Lesson> GetLessonWithQuestionsAndAnswersAsync(int lessonId)
        {
            return await GetAsync(
                condition: l => l.ID == lessonId,
                include: new[] { "Chapter","Questions.Answers" },
                Tracking: false
            );
        }
    }
}
