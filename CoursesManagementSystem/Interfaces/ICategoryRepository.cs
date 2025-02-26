using CoursesManagementSystem.DB.Models;

namespace CoursesManagementSystem.Interfaces
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<Category> GetByIdAsync(int id);
    }
}
