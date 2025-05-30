﻿using CoursesManagementSystem.DB.Models;

namespace CoursesManagementSystem.Interfaces
{
    public interface ICourseRepository : IBaseRepository<Course>
    {
        Task<Course?> GetCourseWithConfigsAsync(int courseId);
    }
}
