using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace CoursesManagementSystem.Controllers
{
    public class CourseQuestionConfigController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CourseQuestionConfigController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courseQuestionConfigs = await unitOfWork.CourseQuestionConfigRepository.GetAllAsync(
                c => !c.IsDeleted && c.CreatedBy == User.Identity.Name
                , new[] { "Course", "QuestionLevel" }
            );

            return View(courseQuestionConfigs);
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int Courseid)
        {
            await PopulateDropdowns();
            ViewBag.CourseId = Courseid;
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseQuestionConfig courseQuestionConfig)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                ViewBag.CourseId = courseQuestionConfig.CourseId;
                return View(courseQuestionConfig);
            }

            var existingConfig = await unitOfWork.CourseQuestionConfigRepository
                .GetAsync(c => c.CourseId == courseQuestionConfig.CourseId
                             && c.QuestionLevelId == courseQuestionConfig.QuestionLevelId && c.CreatedBy == User.Identity.Name);

            if (existingConfig != null)
            {
                if (existingConfig.IsDeleted)
                {
                    existingConfig.IsDeleted = false;
                    existingConfig.QuestionsCountPerLesson = courseQuestionConfig.QuestionsCountPerLesson;
                    existingConfig.LastModifiedAt = DateTime.UtcNow;
                    existingConfig.LastModifiedBy = User.Identity.Name ?? "System";

                    unitOfWork.CourseQuestionConfigRepository.Update(existingConfig);
                    await unitOfWork.CompleteAsync();

                    return RedirectToAction("CourseQuestionConfigByCourseId", new { Courseid = courseQuestionConfig.CourseId });

                }

                ModelState.AddModelError("QuestionLevelId", "A configuration already exists for this course with The Same Difficulty Level.");
                await PopulateDropdowns();
                ViewBag.CourseId = courseQuestionConfig.CourseId;
                return View(courseQuestionConfig);
            }

            courseQuestionConfig.CreatedAt = DateTime.UtcNow;
            courseQuestionConfig.CreatedBy = User?.Identity.Name ?? "System";

            await unitOfWork.CourseQuestionConfigRepository.AddAsync(courseQuestionConfig);
            await unitOfWork.CompleteAsync();
            return RedirectToAction("CourseQuestionConfigByCourseId", new { Courseid = courseQuestionConfig.CourseId });

        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int Courseid)
        {
            var courseQuestionConfig = await unitOfWork.CourseQuestionConfigRepository.GetAsync(
                c => c.ID == id && c.CreatedBy == User.Identity.Name, new[] { "Course", "QuestionLevel" });

            if (courseQuestionConfig == null)
            {
                TempData["Error"] = "Course Question Configuration not found!";
                return RedirectToAction("CourseQuestionConfigByCourseId", new { Courseid = Courseid });
            }

            await PopulateDropdowns();
            ViewBag.CourseId = Courseid;
            return View(courseQuestionConfig);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseQuestionConfig courseQuestionConfig)
        {
            if (id != courseQuestionConfig.ID)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                ViewBag.CourseId = courseQuestionConfig.CourseId;
                return View(courseQuestionConfig);
            }

            var existingConfig = await unitOfWork.CourseQuestionConfigRepository.GetAsync(c => c.ID == id && c.CreatedBy == User.Identity.Name);

            if (existingConfig == null)
            {
                TempData["Error"] = "Course Question Configuration not found!";
                return RedirectToAction("CourseQuestionConfigByCourseId", new { Courseid = courseQuestionConfig.CourseId });

            }
            var duplicate = await unitOfWork.CourseQuestionConfigRepository
               .GetAsync(c => c.ID!= courseQuestionConfig.ID && c.CourseId == courseQuestionConfig.CourseId
                            && c.QuestionLevelId == courseQuestionConfig.QuestionLevelId && c.CreatedBy == User.Identity.Name);
            if (duplicate != null)
            {
                ModelState.AddModelError("QuestionLevelId", "A configuration already exists for this course with The Same Difficulty Level.");
                await PopulateDropdowns();
                ViewBag.CourseId = courseQuestionConfig.CourseId;
                return View(courseQuestionConfig);
            }

            existingConfig.QuestionsCountPerLesson = courseQuestionConfig.QuestionsCountPerLesson;
            existingConfig.CourseId = courseQuestionConfig.CourseId;
            existingConfig.QuestionLevelId = courseQuestionConfig.QuestionLevelId;
            existingConfig.LastModifiedAt = DateTime.UtcNow;
            existingConfig.LastModifiedBy = User.Identity.Name ?? "System";

            await unitOfWork.CompleteAsync();

            return RedirectToAction("CourseQuestionConfigByCourseId", new { Courseid = courseQuestionConfig.CourseId });

        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var courseQuestionConfig = await unitOfWork.CourseQuestionConfigRepository
                .GetAsync(q => q.ID == id && q.CreatedBy == User.Identity.Name);

            if (courseQuestionConfig == null || courseQuestionConfig.IsDeleted)
            {
                TempData["Error"] = "Course Question Configuration not found or already deleted.";
                return Json(new { success = false });
            }

            courseQuestionConfig.IsDeleted = true;
            await unitOfWork.CompleteAsync();

            TempData["Success"] = "Course Question Configuration deleted successfully.";
            return Json(new { success = true });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var courseQuestionConfig = await unitOfWork.CourseQuestionConfigRepository
                .GetAsync(q => !q.IsDeleted && q.CreatedBy == User.Identity.Name && q.ID == id, new[] { "Course", "QuestionLevel" });

            if (courseQuestionConfig == null)
            {
                TempData["Error"] = "No Course Question Configuration found with the provided ID.";
                return RedirectToAction("Index", "Course");
            }

            return View(courseQuestionConfig);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CourseQuestionConfigByCourseId(int Courseid)
        {
            ViewBag.CourseId = Courseid;
            var courseQuestionConfig = await unitOfWork.CourseQuestionConfigRepository
                .GetAllAsync(q => !q.IsDeleted && q.CreatedBy == User.Identity.Name && q.CourseId == Courseid, new[] { "Course", "QuestionLevel" });

            if (courseQuestionConfig == null)
            {
                TempData["Error"] = "No Question Configs found for the provided Course ID.";
                return RedirectToAction("Index", "Course");
            }

            return View(courseQuestionConfig);
        }




        private async Task PopulateDropdowns()
        {
            var courses = await unitOfWork.CourseRepository.GetAllAsync(ql => !ql.IsDeleted && ql.CreatedBy == User.Identity.Name) ?? new List<Course>();
            var questionLevels = await unitOfWork.QuestionLevelRepository.GetAllAsync(ql=>!ql.IsDeleted ) ?? new List<QuestionLevel>();

            ViewBag.Courses = new SelectList(courses, "ID", "Name");
            ViewBag.QuestionLevels = new SelectList(questionLevels, "ID", "Name");
        }
    }
}
