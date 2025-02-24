using CoursesManagementSystem.Data;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;

namespace CoursesManagementSystem.Repository
{
    public class AnswerRepository:BaseRepository<Answer>,IAnswerRepository
    {
        private readonly ApplicationDbContext _context;
        public AnswerRepository(ApplicationDbContext _context) : base(_context)
        {
            this._context = _context;
        }
    }
}
