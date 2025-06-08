using AutoMapper;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
          
            
            var res=await unitOfWork.LevelRepository
                .GetAllQuery(l=>!l.IsDeleted)
                .Select(c=>new LevelsWithAssociatedCoursesVM
                {
                ID = c.ID,
                Name = c.Name,
                Sort = c.Sort,
                AssociatedCourses = c.Courses.Count(c=>!c.IsDeleted)         
                })
                .ToListAsync();

            return View(res);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(LevelVM levelVM)
        {
            if (ModelState.IsValid)
            {
                //check if it is unique
                var Level = await unitOfWork.LevelRepository
                    .GetAsync(l => !l.IsDeleted && (l.Sort == levelVM.Sort || l.Name == levelVM.Name), null, false);
                if (Level != null)
                {

                    if (Level.Name == levelVM.Name)
                    {

                        ModelState.AddModelError("Name", "Level Name already exists ");

                    }
                    if (Level.Sort == levelVM.Sort)
                    {

                        ModelState.AddModelError("Sort", "Difficulty Number already exists ");

                    }
                    return View(levelVM);
                }
                //check if level is marked deleted -->mark undeleted
                var deletedlevel = await unitOfWork.LevelRepository.GetAsync(l => l.IsDeleted  && l.Sort == levelVM.Sort && l.Name == levelVM.Name);
                if (deletedlevel != null)
                {
                    deletedlevel.IsDeleted = false;
                    await unitOfWork.CompleteAsync();
                    return RedirectToAction("Index");

                }
                //new level
                Level levelmp = mapper.Map<Level>(levelVM);
                levelmp.CreatedAt = DateTime.Now;
                levelmp.CreatedBy = User.Identity.Name ?? "System";
                await unitOfWork.LevelRepository.AddAsync(levelmp);
                await unitOfWork.CompleteAsync();
                return RedirectToAction("Index");

            }

            return View(levelVM);

        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Update(int id)
        {
            var Level = await unitOfWork.LevelRepository.GetByIdAsync(id);
            if (Level == null)
            {
                TempData["Error"] = "No Level with This Id is Found";
                return RedirectToAction("Index");
            }
            LevelVM res = mapper.Map<LevelVM>(Level);
            ViewBag.Id = id;
            return View(res);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Update(int id, LevelVM levelVM)
        {
            if (ModelState.IsValid)
            {

                Level lv = await unitOfWork.LevelRepository.GetByIdAsync(id);
                if (lv == null)
                {
                    TempData["Error"] = "No Level with This Id is Found";
                    return RedirectToAction("Index");
                }
                //check if it is unique
                var Level = await unitOfWork.LevelRepository
                    .GetAsync(l => !l.IsDeleted && l.Sort == levelVM.Sort && l.Name == levelVM.Name, null, false);
                if (Level != null)
                {
                    ModelState.AddModelError("Name", "Level Name already exists ");
                    ModelState.AddModelError("Sort", "Difficulty Number already exists ");
                    return View(levelVM);
                }


                var Lv = await unitOfWork.LevelRepository //we want non duplicate sort
                    .GetAsync(l => !l.IsDeleted && l.Sort == levelVM.Sort, null, false);
                if (Lv != null)
                {
                    ModelState.AddModelError("Sort", "Difficulty Number already exists ");
                    return View(levelVM);
                }

                //check if level is marked deleted -->mark undeleted
                var deletedlevel = await unitOfWork.LevelRepository.GetAsync(l => l.IsDeleted && l.Sort == levelVM.Sort && l.Name == levelVM.Name);
                if (deletedlevel != null)
                {
                    lv.IsDeleted = true;
                    deletedlevel.IsDeleted = false;
                    await unitOfWork.CompleteAsync();
                    return RedirectToAction("Index");

                }
                lv.LastModifiedAt = DateTime.Now;
                lv.LastModifiedBy = User.Identity.Name ?? "System";
                lv.Sort = levelVM.Sort;
                await unitOfWork.CompleteAsync();
                return RedirectToAction("Index");

            }

            return View(levelVM);

        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> getById(int id)
        {
           Level l = await unitOfWork.LevelRepository.GetAsync(c => !c.IsDeleted && c.ID == id);
            if (l == null)
            {
                // return NotFound();

                TempData["Error"] = "No Level with This Id is Found";
                return RedirectToAction("Index");
            }
            //map level to levelDetailsVM
           LevelDetailsVM levmp= mapper.Map<LevelDetailsVM>(l);
           
            return View(levmp);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            Level l = await unitOfWork.LevelRepository.GetAsync(l => !l.IsDeleted && l.ID == id);
            if (l == null)
            {
                //return NotFound();

                TempData["Error"] = "No Level with this Id is Found";
                return Json(new { success = false });
            }
            //check if Level is not assigned to a course
            var coursewithlevelfound = await unitOfWork.CourseRepository.GetAsync(c => !c.IsDeleted && l.CreatedBy == User.Identity.Name && c.LevelId == id);

            if (coursewithlevelfound != null)
            {
                //return BadRequest();

                TempData["Error"] = "Can not delete a Level that is assigned to a Course";
                return Json(new { success = false });
            }

            l.IsDeleted = true;
            await unitOfWork.CompleteAsync();
            TempData["Success"] = "Level Deleted Successfully";
            return Json(new { success = true });


        }

    }
}
