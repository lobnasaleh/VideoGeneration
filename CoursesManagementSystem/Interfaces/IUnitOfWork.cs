namespace CoursesManagementSystem.Interfaces
{
    public interface IUnitOfWork:IDisposable    
    {
        public IAnswerRepository AnswerRepository { get ; }
        public ICategoryRepository CategoryRepository { get; }
        public IChapterRepository ChapterRepository { get; }
        public ICourseConfigRepository CourseConfigRepository { get; }
        public ICourseQuestionConfigRepository CourseQuestionConfigRepository { get; }
        public ICourseRepository CourseRepository { get; }
        public ILessonRepository LessonRepository { get; }
        public ILevelRepository LevelRepository { get; }
        public IQuestionLevelRepository QuestionLevelRepository { get; }
        public IQuestionRepository QuestionRepository { get; }

        public Task<int> CompleteAsync();
    }
}
