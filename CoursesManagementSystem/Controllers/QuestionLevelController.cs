using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoursesManagementSystem.Controllers
{
    public class QuestionLevelController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public QuestionLevelController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;

        }
        public async Task<IActionResult> GetAll()
        {
            var questionLevels = await unitOfWork.QuestionLevelRepository.GetAllAsync(c => !c.IsDeleted);

            return View(questionLevels);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(QuestionLevel questionLevel)
        {
            if (!ModelState.IsValid)
            {
                return View(questionLevel);
            }

            var existingLevel = await unitOfWork.QuestionLevelRepository
                .GetAsync(q => q.Name == questionLevel.Name);

            if (existingLevel != null)
            {
                if (existingLevel.IsDeleted)
                {
                    
                    existingLevel.IsDeleted = false;
                    existingLevel.DifficultyScore = questionLevel.DifficultyScore; 
                    await unitOfWork.CompleteAsync();
                    return RedirectToAction(nameof(GetAll));
                }

                ModelState.AddModelError("Name", "A Question Level with this name already exists.");
                return View(questionLevel);
            }

            if (questionLevel.DifficultyScore < 1 || questionLevel.DifficultyScore > 100)
            {
                ModelState.AddModelError("DifficultyScore", "Difficulty score must be between 1 and 100.");
                return View(questionLevel);
            }

            questionLevel.CreatedAt = DateTime.UtcNow;
            await unitOfWork.QuestionLevelRepository.AddAsync(questionLevel);
            await unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(GetAll));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // 1️⃣ Check if the QuestionLevel exists
            var questionLevel = await unitOfWork.QuestionLevelRepository.GetByIdAsync(id);

            if (questionLevel == null)
            {
                return NotFound("The requested Question Level does not exist.");
            }

            return View(questionLevel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, QuestionLevel questionLevel)
        {
            if (id != questionLevel.ID)
            {
                return BadRequest("Invalid request.");
            }

            if (!ModelState.IsValid)
            {
                return View(questionLevel);
            }

            var existingLevel = await unitOfWork.QuestionLevelRepository.GetByIdAsync(id);

            if (existingLevel == null)
            {
                return NotFound("The requested Question Level does not exist.");
            }

            var duplicateLevel = await unitOfWork.QuestionLevelRepository
                .GetAsync(q => q.Name == questionLevel.Name && q.ID != id);

            if (duplicateLevel != null)
            {
                ModelState.AddModelError("Name", "This level name already exists.");
                return View(questionLevel);
            }

            if (existingLevel.IsDeleted)
            {
                existingLevel.IsDeleted = false;
            }

            existingLevel.LastModifiedAt = DateTime.UtcNow;
            existingLevel.LastModifiedBy = questionLevel.LastModifiedBy;
            await unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(GetAll));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var questionLevel = await unitOfWork.QuestionLevelRepository
                .GetAsync(q => !q.IsDeleted && q.ID == id);

            if (questionLevel == null)
            {
                TempData["Error"] = "No Question Level found with the provided ID.";
                return RedirectToAction(nameof(GetAll));
            }

            return View(questionLevel); // Show confirmation page
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var questionLevel = await unitOfWork.QuestionLevelRepository
                .GetAsync(q => !q.IsDeleted && q.ID == id);

            if (questionLevel == null)
            {
                TempData["Error"] = "No Question Level found with the provided ID.";
                return RedirectToAction(nameof(GetAll));
            }

            questionLevel.IsDeleted = true; // Soft delete
            await unitOfWork.CompleteAsync();

            TempData["Success"] = "Question Level deleted successfully.";
            return RedirectToAction(nameof(GetAll));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var questionLevel = await unitOfWork.QuestionLevelRepository
                .GetAsync(q => !q.IsDeleted && q.ID == id);

            if (questionLevel == null)
            {
                TempData["Error"] = "No Question Level found with the provided ID.";
                return RedirectToAction(nameof(GetAll));
            }

            return View(questionLevel); // Return details page
        }





    }
}
