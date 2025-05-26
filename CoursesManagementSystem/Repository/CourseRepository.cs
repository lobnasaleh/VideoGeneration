using CoursesManagementSystem.Data;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoursesManagementSystem.Repository
{
    public class CourseRepository:BaseRepository<Course>,ICourseRepository
    {
        private readonly ApplicationDbContext _context;
        public CourseRepository(ApplicationDbContext _context) : base(_context)
        {
            this._context = _context;
        }

        public async Task<Course?> GetCourseWithConfigsAsync(int courseId)
        {
            return await _context.Courses
        .Include(c => c.CourseConfig)
        .Include(c => c.CourseQuestionsConfig)
            .ThenInclude(q => q.QuestionLevel)
        .FirstOrDefaultAsync(c => c.ID == courseId);
        }
    }
}
