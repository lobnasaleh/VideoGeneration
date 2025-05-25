using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using CoursesManagementSystem.DTOs;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.ViewModels;

namespace CoursesManagementSystem.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> AddRole(string name);//mapped from dtos to avoid Entities referencing to  
        Task<List<IdentityRole>> GetRoles();//mapped from dtos to avoid Entities referencing to  
        Task<AuthResponse> Register(RegisterVM registerRequest);//mapped from dtos to avoid Entities referencing to  
         Task<AuthResponse> Login(LoginVM loginRequest);
        Task<AuthResponse> Logout();
        Task<ApplicationUser> getUserById(string userId);
    }
}
