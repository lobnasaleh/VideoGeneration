using AutoMapper;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CoursesManagementSystem.Controllers
{
    public class LevelController : Controller
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public LevelController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
           var res= await unitOfWork.LevelRepository.GetAllAsync(l=>!l.IsDeleted);
            return View(res);
        }
        [HttpPost]
        public async Task<IActionResult> Create(LevelVM levelVM)
        {

            //check if it 


            return View(levelVM);
           
        }

    }
}
