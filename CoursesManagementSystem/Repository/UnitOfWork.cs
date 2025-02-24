using CoursesManagementSystem.Data;
using CoursesManagementSystem.Interfaces;

namespace CoursesManagementSystem.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext _context)
        {
            this._context = _context;
            AnswerRepository=new AnswerRepository(_context);
            CategoryRepository = new CategoryRepository(_context);
            ChapterRepository = new ChapterRepository(_context);
            CourseConfigRepository = new CourseConfigRepository(_context);
            CourseQuestionConfigRepository = new CourseQuestionConfigRepository(_context);
            CourseRepository = new CourseRepository(_context);
            LessonRepository = new LessonRepository(_context);
            LevelRepository = new LevelRepository(_context);
            QuestionLevelRepository = new QuestionLevelRepository(_context);
            QuestionRepository = new QuestionRepository(_context);
        }

        public IAnswerRepository AnswerRepository { get;private set; }

        public ICategoryRepository CategoryRepository { get; private set; }

        public IChapterRepository ChapterRepository { get; private set; }

        public ICourseConfigRepository CourseConfigRepository { get; private set; }

        public ICourseQuestionConfigRepository CourseQuestionConfigRepository { get; private set; }

        public ICourseRepository CourseRepository { get; private set; }

        public ILessonRepository LessonRepository { get; private set; }

        public ILevelRepository LevelRepository { get; private set; }

        public IQuestionLevelRepository QuestionLevelRepository { get; private set; }

        public IQuestionRepository QuestionRepository { get; private set; }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        } 
    }
}
