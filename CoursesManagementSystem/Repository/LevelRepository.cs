using CoursesManagementSystem.Data;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;

namespace CoursesManagementSystem.Repository
{
    public class LevelRepository:BaseRepository<Level>,ILevelRepository
    {
        private readonly ApplicationDbContext _context;
        public LevelRepository(ApplicationDbContext _context):base(_context) 
        {
            this._context = _context;
        }
    }
}
