using CoursesManagementSystem.Data;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoursesManagementSystem.Repository
{
    public class CategoryRepository:BaseRepository<Category>,ICategoryRepository
    {
        private readonly ApplicationDbContext context;
        public CategoryRepository(ApplicationDbContext context):base(context) 
        {
            this.context = context;
        }
        public async Task<Category> GetByIdAsync(int id)
        {
            return await context.Categories.FindAsync(id);
        }
    }
}
