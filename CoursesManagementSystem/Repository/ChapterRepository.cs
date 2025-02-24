using CoursesManagementSystem.Data;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;

namespace CoursesManagementSystem.Repository
{
    public class ChapterRepository:BaseRepository<Chapter>,IChapterRepository
    {
        private readonly ApplicationDbContext _context;
        public ChapterRepository(ApplicationDbContext _context) : base(_context)
        {
            this._context = _context;
        }
    }
}
