using CoursesManagementSystem.Data;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoursesManagementSystem.Repository
{
    public class LessonRepository:BaseRepository<Lesson>,ILessonRepository
    {
        private readonly ApplicationDbContext _context;
        public LessonRepository(ApplicationDbContext _context) : base(_context)
        {
            this._context = _context;
        }

       /* public async Task<Lesson> GetLessonWithQuestionsAndAnswersAsync(int lessonId)
        {
            return await GetAsync(
                condition: l => l.ID == lessonId,
                include: new[] { "Chapter","Questions.Answers" },
                Tracking: false
            );
        }*/

        public async Task<Lesson> GetLessonWithQuestionsAndAnswersAsync(int lessonId)
        {
            return await _context.Lessons
                .Include(l => l.Chapter)
                .Include(l => l.Questions.Where(q => !q.IsDeleted))
                    .ThenInclude(q => q.Answers.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync(l => l.ID == lessonId);
        }
    }
}
