using CoursesManagementSystem.Data;
using CoursesManagementSystem.Interfaces;

namespace CoursesManagementSystem.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IAnswerRepository _AnswerRepository;
        private ICategoryRepository _CategoryRepository;
        private IChapterRepository _ChapterRepository;
        private ICourseConfigRepository _CourseConfigRepository;
        private ICourseQuestionConfigRepository _CourseQuestionConfigRepository;
        private ICourseRepository _CourseRepository;
        private ILessonRepository _LessonRepository;
        private ILevelRepository _LevelRepository;
        private IQuestionLevelRepository _QuestionLevelRepository;
        private IQuestionRepository _QuestionRepository;



        public UnitOfWork(ApplicationDbContext _context)
        {
            this._context = _context;
        }

        public IAnswerRepository AnswerRepository
        {
            get
            {
                _AnswerRepository ??= new AnswerRepository(_context);
                return _AnswerRepository;
            }

        }
        public ICategoryRepository CategoryRepository
        {
            get
            {
                _CategoryRepository ??= new CategoryRepository(_context);
                return _CategoryRepository;

            }
        }

        public IChapterRepository ChapterRepository
        {
            get
            {
                _ChapterRepository ??= new ChapterRepository(_context);
                return _ChapterRepository;

            }
        }

        public ICourseConfigRepository CourseConfigRepository
        {
            get
            {
                _CourseConfigRepository ??= new CourseConfigRepository(_context);
                return _CourseConfigRepository;

            }
        }

        public ICourseQuestionConfigRepository CourseQuestionConfigRepository
        {
            get
            {
                _CourseQuestionConfigRepository ??= new CourseQuestionConfigRepository(_context);
                return _CourseQuestionConfigRepository;

            }
        }

        public ICourseRepository CourseRepository
        {
            get
            {
                _CourseRepository ??= new CourseRepository(_context);
                return _CourseRepository;

            }
        }

        public ILessonRepository LessonRepository
        {
            get
            {
                _LessonRepository ??= new LessonRepository(_context);
                return _LessonRepository;

            }
        }

        public ILevelRepository LevelRepository
        {
            get
            {
                _LevelRepository ??= new LevelRepository(_context);
                return _LevelRepository;
            }
        }

        public IQuestionLevelRepository QuestionLevelRepository
        {
            get
            {

                _QuestionLevelRepository ??= new QuestionLevelRepository(_context);
                return _QuestionLevelRepository;
            }
        }

        public IQuestionRepository QuestionRepository
        {
            get
            {
                _QuestionRepository ??= new QuestionRepository(_context);
                return _QuestionRepository;
            }
        }

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
