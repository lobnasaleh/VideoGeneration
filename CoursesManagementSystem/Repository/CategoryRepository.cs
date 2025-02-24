using CoursesManagementSystem.Data;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;

namespace CoursesManagementSystem.Repository
{
    public class CategoryRepository:BaseRepository<Category>,ICategoryRepository
    {
        private readonly ApplicationDbContext context;
        public CategoryRepository(ApplicationDbContext context):base(context) 
        {
            this.context = context;
        }
    }
}
