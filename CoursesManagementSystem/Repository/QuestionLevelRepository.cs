using CoursesManagementSystem.Data;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;

namespace CoursesManagementSystem.Repository
{
    public class QuestionLevelRepository:BaseRepository<QuestionLevel>,IQuestionLevelRepository
    {
        private readonly ApplicationDbContext _context;
        public QuestionLevelRepository(ApplicationDbContext _context):base(_context) {
        
        this._context = _context;
        }


    }
}
