using CoursesManagementSystem.Data;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;

namespace CoursesManagementSystem.Repository
{
    public class QuestionRepository:BaseRepository<Question>,IQuestionRepository
    {
        private readonly ApplicationDbContext _context;
        public QuestionRepository(ApplicationDbContext _context):base(_context)
        {
            this._context = _context;
        }
    }
}
