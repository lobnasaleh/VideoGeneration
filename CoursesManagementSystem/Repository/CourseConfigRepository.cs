using CoursesManagementSystem.Data;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;

namespace CoursesManagementSystem.Repository
{
    public class CourseConfigRepository:BaseRepository<CourseConfig>,ICourseConfigRepository
    {
        private readonly ApplicationDbContext _context;
        public CourseConfigRepository(ApplicationDbContext _context) : base(_context)
        {
            this._context = _context;
        }
    }
}
