using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.DTOs;
using Microsoft.EntityFrameworkCore;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.ViewModels;

namespace CoursesManagementSystem.Services
{
        public class AuthService : IAuthService
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly RoleManager<IdentityRole> roleManager;
            private readonly SignInManager<ApplicationUser> signInManager;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly IConfiguration _config;


            public AuthService(UserManager<ApplicationUser> _userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, IConfiguration _config, IHttpContextAccessor httpContextAccessor)
            {
                this._userManager = _userManager;
                this.roleManager = roleManager;
                this.signInManager = signInManager;
                this._config = _config;
                _httpContextAccessor = httpContextAccessor;
            }
            public async Task<ApplicationUser> getUserById(string userId)
            {
                return await _userManager.FindByIdAsync(userId);
            }

            public async Task<IEnumerable<ApplicationUser>> getAllUsers()
            {
                return await _userManager.GetUsersInRoleAsync("User");
            }

            public async Task<AuthResponse> Login(LoginVM loginRequest)
            {
                var user = await _userManager.FindByEmailAsync(loginRequest.Email);
                if (user is null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
                {
                    AuthResponse response = new AuthResponse() { isAuthenticated = false, Message = "Email Or Password is Incorrect" };
                    return response;
                }

                //get roles of user 
                var userRoles = await _userManager.GetRolesAsync(user);
                List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));

                }

                /// Create a claims identity
                var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Sign in the user
                await _httpContextAccessor.HttpContext.SignInAsync(
                    IdentityConstants.ApplicationScheme,
                   claimsPrincipal
                        );
                AuthResponse res = new AuthResponse()
                {

                    isAuthenticated = true,
                    Message = "Logged In Successfully !",
                    Email = loginRequest.Email,
                    Roles = userRoles,
                    Username = user.UserName
                };

                return res;

            }
          public async Task<AuthResponse> Register(RegisterVM registerRequest)
        {
                if (await _userManager.FindByEmailAsync(registerRequest.Email) is not null)//we found a user with this email
                {
                    return new AuthResponse() { isAuthenticated = false, Message = "This Email is already Registered" };
                }

            //map RegisterRequest to ApplicationUser
            var user = new ApplicationUser()
            {
                Email = registerRequest.Email,
                UserName = registerRequest.Email
            };
                var errorslist = new List<string>();
                var res = await _userManager.CreateAsync(user, registerRequest.ConfirmPassword);
                if (!res.Succeeded)
                {
                    foreach (var err in res.Errors)
                    {
                        errorslist.Add(err.Description);
                    }

                    string errormessages = string.Join(", ", errorslist);

                    return new AuthResponse()
                    {
                        isAuthenticated = false,
                        Message = errormessages
                    };
                }
                //assign role
                await _userManager.AddToRoleAsync( user, "User");

                List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

                claims.Add(new Claim(ClaimTypes.Role, "User"));

                // await signInManager.SignInWithClaimsAsync(user, false, claims);
                // Create a claims identity
                /// Create a claims identity
                var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Sign in the user
                await _httpContextAccessor.HttpContext.SignInAsync(
                    IdentityConstants.ApplicationScheme,
                   claimsPrincipal
                        );
                return new AuthResponse
                {
                    Email = user.Email,
                    isAuthenticated = true,
                    Message = $"Welcome {user.UserName}",
                    Roles = new List<string> { "User" },
                    Username = user.UserName
                };
            }

          /*  public async Task<AuthResponse> UpdateProfile(string id, UpdateRequest UpdateRequest)
            {
                AuthResponse auth = new AuthResponse();
                //registerRequest.
                Patient p = await _userManager.FindByIdAsync(id) as Patient;
                if (p == null)
                {
                    auth.Message = "No Patient With This ID is found";
                    auth.IsSuccess = false;
                    return auth;
                }
                //updateRequestVM-->Patient
                p.Address = UpdateRequest.Address;
                p.FullName = UpdateRequest.FullName;
                p.InsuranceProvider = UpdateRequest.InsuranceProvider;
                p.InsuranceNumber = UpdateRequest.InsuranceNumber;
                p.DOB = UpdateRequest.DOB;
                p.EmergencyContact = UpdateRequest.EmergencyContact;

                var result = await _userManager.UpdateAsync(p);

                if (result.Succeeded)
                {
                    auth.Message = "Staff Info Updated Successfully";
                    auth.IsSuccess = true;
                }
                else
                {
                    auth.Errors = result.Errors.ToList();

                    auth.Message = "Failed to Update Staff Info";
                    auth.IsSuccess = false;
                }
                return auth;
            }*/

            public async Task<AuthResponse> AddRole(string name)
            {
                IdentityRole role = new IdentityRole();
                role.Name = name;
                var res = await roleManager.CreateAsync(role);
                if (res.Succeeded)
                {
                    return new AuthResponse() { IsSuccess = true };
                }
                return new AuthResponse() { IsSuccess = false };
            }

            public async Task<List<IdentityRole>> GetRoles()
            {
                var res = await roleManager.Roles.ToListAsync();
                return res;
            }

            public async Task<AuthResponse> Logout()
            {
                await signInManager.SignOutAsync();
                return new AuthResponse() { IsSuccess = true };
            }

       
    }
    }


