using AutoMapper;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.ViewModels;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace CoursesManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService authService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public AuthController(IAuthService authService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            this.authService = authService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;

        }
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var res = await authService.GetRoles();

            return View(res);
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginRequestvm)
        {

            if (!ModelState.IsValid)
            {
                return View(loginRequestvm);
            }
        
            var result = await authService.Login(loginRequestvm);

            if (!result.isAuthenticated)
            {
                ViewBag.Error = result.Message;
                ModelState.AddModelError("",result.Message);
                return View(loginRequestvm);
            }

            return RedirectToAction("getGeneratedCourses", "Course");

        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerRequestVM)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", registerRequestVM);
            }

            var res = await authService.Register(registerRequestVM);

            if (!res.isAuthenticated)
            {
                ModelState.AddModelError("", res.Message);
                return View(registerRequestVM);
            }

            return RedirectToAction("getGeneratedCourses", "Course");
        }
      /*  //update 
        [HttpGet]
        public async Task<IActionResult> Update()
        {
            //getting the logged in user 
            string loggedinuser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Patient p = await unitOfWork.patientRepository.getAsync(s => !s.IsDeleted && s.Id == loggedinuser, false);
            if (p == null)
            {
                return NotFound();
            }

            UpdateRequestVM pamp = new UpdateRequestVM()
            {
                FullName = p.FullName,
                DOB = p.DOB,
                InsuranceNumber = p.InsuranceNumber,
                InsuranceProvider = p.InsuranceProvider,
                Address = p.Address,
                EmergencyContact = p.EmergencyContact
            };//from staff to staffmp

            return View(pamp);
        }*/

   /*     [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateRequestVM pFromReq)
        {

            if (ModelState.IsValid)
            {

                //var sttt = mapper.Map<UpateStaffRequest>(staffFromReq);
                UpdateRequest up = new UpdateRequest()
                {
                    Address = pFromReq.Address,
                    EmergencyContact = pFromReq.EmergencyContact,
                    InsuranceProvider = pFromReq.InsuranceProvider,
                    InsuranceNumber = pFromReq.InsuranceNumber,
                    DOB = pFromReq.DOB,
                    FullName = pFromReq.FullName
                };

                var loggedinuser = User.FindFirstValue(ClaimTypes.NameIdentifier);


                var res = await authService.UpdateProfile(loggedinuser, up);
                if (res.IsSuccess)
                {
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    ViewData["Error"] = res.ToString();
                    return View(pFromReq);
                }

            }

            return View(pFromReq);

        }*/
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await authService.Logout();

            return RedirectToAction("Login", "Auth");
        }

    }
}
