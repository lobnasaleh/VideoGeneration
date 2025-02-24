using CoursesManagementSystem.DB.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoursesManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

   
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseConfig> CourseConfigs { get; set; }
        public DbSet<CourseQuestionConfig> CourseQuestionConfigs { get; set; }
        public DbSet<Lesson> Lessons{ get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionLevel> QuestionsLevel { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           /* modelBuilder.Entity<CourseConfig>()
                .HasOne(cc => cc.Course)
                .WithOne(c=>c.CourseConfig)
                .HasForeignKey<CourseConfig>(c => c.CourseId);


            modelBuilder.Entity<CourseQuestionConfig>()
              .HasOne(cqc => cqc.QuestionLevel)
              .WithOne(ql => ql.CourseQuestionConfig)
              .HasForeignKey<CourseQuestionConfig>(c => c.QuestionLevelId);
*/


            base.OnModelCreating(modelBuilder);
        }

    }
}
