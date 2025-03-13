using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Enums;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoursesManagementSystem.Controllers
{
    public class CourseConfigController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CourseConfigController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;

        }

        public async Task<IActionResult> GetAll()
        {
            var courseConfigs = await unitOfWork.CourseConfigRepository
                .GetAllAsync(c => !c.IsDeleted, new string[] { "Course" });

            return View(courseConfigs);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PopulateDropdowns(); 
            return View();
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseConfig courseConfig)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(); 
                return View(courseConfig);
            }

            var existingConfig = await unitOfWork.CourseConfigRepository
                .GetAsync(c => c.CourseId == courseConfig.CourseId);

            if (existingConfig != null)
            {
                if (existingConfig.IsDeleted)
                {
                    existingConfig.IsDeleted = false;
                    existingConfig.ChaptersCount = courseConfig.ChaptersCount;
                    existingConfig.LessonsCountPerChapter = courseConfig.LessonsCountPerChapter;
                    existingConfig.VideoDurationInMin = courseConfig.VideoDurationInMin;
                    existingConfig.Language = courseConfig.Language;
                    existingConfig.Persona = courseConfig.Persona;

                    existingConfig.LastModifiedAt = DateTime.UtcNow;
                    existingConfig.LastModifiedBy = User.Identity.Name ?? "System";

                    unitOfWork.CourseConfigRepository.Update(existingConfig);
                    await unitOfWork.CompleteAsync();

                    return RedirectToAction(nameof(GetAll));
                }
                ModelState.AddModelError("CourseId", "A configuration already exists for this course.");
                PopulateDropdowns();
                return View(courseConfig);
            }
            courseConfig.CreatedAt = DateTime.UtcNow;
            courseConfig.CreatedBy = User.Identity.Name ?? "System";
            await unitOfWork.CourseConfigRepository.AddAsync(courseConfig);
            await unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(GetAll));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var courseConfig = await unitOfWork.CourseConfigRepository.GetAsync(c => c.ID == id, new[] { "Course" });

            if (courseConfig == null)
            {
                TempData["Error"] = "Course Configuration not found!";
                return RedirectToAction(nameof(GetAll));
            }

            PopulateDropdowns();
            ViewBag.Courses = new SelectList(await unitOfWork.CourseRepository.GetAllAsync(), "ID", "Name", courseConfig.CourseId);

            return View(courseConfig);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseConfig courseConfig)
        {
            if (id != courseConfig.ID)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Courses = new SelectList(await unitOfWork.CourseRepository.GetAllAsync(), "ID", "Name", courseConfig.CourseId);
                return View(courseConfig);
            }

            var existingConfig = await unitOfWork.CourseConfigRepository.GetAsync(c => c.ID == id);

            if (existingConfig == null)
            {
                TempData["Error"] = "Course Configuration not found!";
                return RedirectToAction(nameof(GetAll));
            }

            existingConfig.ChaptersCount = courseConfig.ChaptersCount;
            existingConfig.LessonsCountPerChapter = courseConfig.LessonsCountPerChapter;
            existingConfig.VideoDurationInMin = courseConfig.VideoDurationInMin;
            existingConfig.Language = courseConfig.Language;
            existingConfig.Persona = courseConfig.Persona;
            existingConfig.CourseId = courseConfig.CourseId ; 
            existingConfig.LastModifiedAt = DateTime.UtcNow;
            existingConfig.LastModifiedBy = User.Identity.Name ?? "System";

            await unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(GetAll));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var courseConfig = await unitOfWork.CourseConfigRepository
                .GetAsync(q => !q.IsDeleted && q.ID == id, new[] { "Course" });

            if (courseConfig == null)
            {
                TempData["Error"] = "No Course Configurations found with the provided ID.";
                return RedirectToAction(nameof(GetAll));
            }

            return View(courseConfig);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var courseConfig = await unitOfWork.CourseConfigRepository
                .GetAsync(q => q.ID == id); 

            if (courseConfig == null || courseConfig.IsDeleted)
            {
                TempData["Error"] = "Course Config not found or already deleted.";
                return RedirectToAction(nameof(GetAll));
            }
            
           /* var relatedCourse = await unitOfWork.CourseRepository
                .GetAsync(c => !c.IsDeleted && c.ID == courseConfig.CourseId);

            if (relatedCourse != null)
            {
                TempData["Error"] = "Cannot delete Course Config because it is assigned to a Course.";
                return RedirectToAction(nameof(GetAll));
            }*/

            courseConfig.IsDeleted = true;
            await unitOfWork.CompleteAsync();

            TempData["Success"] = "Course Config deleted successfully.";
            return RedirectToAction(nameof(GetAll));
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var courseConfig = await unitOfWork.CourseConfigRepository
                .GetAsync(q => !q.IsDeleted && q.ID == id, new[] { "Course" });

            if (courseConfig == null)
            {
                TempData["Error"] = "No Course Configs found with the provided ID.";
                return RedirectToAction(nameof(GetAll));
            }

            return View(courseConfig); 
        }











        private void PopulateDropdowns()
        {
            ViewBag.Languages = Enum.GetValues(typeof(VideoPresenterLanguageEnum))
                                    .Cast<VideoPresenterLanguageEnum>()
                                    .Select(e => new SelectListItem
                                    {
                                        Value = ((int)e).ToString(),
                                        Text = e.ToString()
                                    });

            ViewBag.Personas = Enum.GetValues(typeof(VideoPresenterPersonaEnum))
                                   .Cast<VideoPresenterPersonaEnum>()
                                   .Select(e => new SelectListItem
                                   {
                                       Value = ((int)e).ToString(),
                                       Text = e.ToString()
                                   });

            ViewBag.Courses = new SelectList(unitOfWork.CourseRepository.GetAllAsync().Result, "ID", "Name");
        }




    }
}
