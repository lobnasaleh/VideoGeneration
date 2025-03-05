using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courseQuestionConfigs = await unitOfWork.CourseQuestionConfigRepository.GetAllAsync(
                c => !c.IsDeleted,
                new string[] { "Course", "QuestionLevel" }
            );

            return View(courseQuestionConfigs);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseQuestionConfig courseQuestionConfig)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(courseQuestionConfig);
            }

            var existingConfig = await unitOfWork.CourseQuestionConfigRepository
                .GetAsync(c => c.CourseId == courseQuestionConfig.CourseId
                             && c.QuestionLevelId == courseQuestionConfig.QuestionLevelId);

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

                    return RedirectToAction(nameof(GetAll));
                }

                ModelState.AddModelError("CourseId", "A configuration already exists for this course.");
                await PopulateDropdowns();
                return View(courseQuestionConfig);
            }

            courseQuestionConfig.CreatedAt = DateTime.UtcNow;
            courseQuestionConfig.CreatedBy = User?.Identity.Name ?? "System";

            await unitOfWork.CourseQuestionConfigRepository.AddAsync(courseQuestionConfig);
            await unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(GetAll));
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var courseQuestionConfig = await unitOfWork.CourseQuestionConfigRepository.GetAsync(
                c => c.ID == id, new[] { "Course", "QuestionLevel" });

            if (courseQuestionConfig == null)
            {
                TempData["Error"] = "Course Question Configuration not found!";
                return RedirectToAction(nameof(GetAll));
            }

            await PopulateDropdowns();
            return View(courseQuestionConfig);
        }


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
                return View(courseQuestionConfig);
            }

            var existingConfig = await unitOfWork.CourseQuestionConfigRepository.GetAsync(c => c.ID == id);

            if (existingConfig == null)
            {
                TempData["Error"] = "Course Question Configuration not found!";
                return RedirectToAction(nameof(GetAll));
            }

            existingConfig.QuestionsCountPerLesson = courseQuestionConfig.QuestionsCountPerLesson;
            existingConfig.CourseId = courseQuestionConfig.CourseId;
            existingConfig.QuestionLevelId = courseQuestionConfig.QuestionLevelId;
            existingConfig.LastModifiedAt = DateTime.UtcNow;
            existingConfig.LastModifiedBy = User.Identity.Name ?? "System";

            await unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(GetAll));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var courseQuestionConfig = await unitOfWork.CourseQuestionConfigRepository
                .GetAsync(q => !q.IsDeleted && q.ID == id, new[] { "Course", "QuestionLevel" });

            if (courseQuestionConfig == null)
            {
                TempData["Error"] = "No Course Question Configurations found with the provided ID.";
                return RedirectToAction(nameof(GetAll));
            }

            return View(courseQuestionConfig);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var courseQuestionConfig = await unitOfWork.CourseQuestionConfigRepository
                .GetAsync(q => q.ID == id);

            if (courseQuestionConfig == null || courseQuestionConfig.IsDeleted)
            {
                TempData["Error"] = "Course Question Configuration not found or already deleted.";
                return RedirectToAction(nameof(GetAll));
            }

            courseQuestionConfig.IsDeleted = true;
            await unitOfWork.CompleteAsync();

            TempData["Success"] = "Course Question Configuration deleted successfully.";
            return RedirectToAction(nameof(GetAll));
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var courseQuestionConfig = await unitOfWork.CourseQuestionConfigRepository
                .GetAsync(q => !q.IsDeleted && q.ID == id, new[] { "Course", "QuestionLevel" });

            if (courseQuestionConfig == null)
            {
                TempData["Error"] = "No Course Question Configuration found with the provided ID.";
                return RedirectToAction(nameof(GetAll));
            }

            return View(courseQuestionConfig);
        }





        private async Task PopulateDropdowns()
        {
            var courses = await unitOfWork.CourseRepository.GetAllAsync() ?? new List<Course>();
            var questionLevels = await unitOfWork.QuestionLevelRepository.GetAllAsync() ?? new List<QuestionLevel>();

            ViewBag.Courses = new SelectList(courses, "ID", "Name");
            ViewBag.QuestionLevels = new SelectList(questionLevels, "ID", "Name");
        }
    }
}
