using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoursesManagementSystem.Controllers
{
    public class AnswerController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public AnswerController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var answers = await unitOfWork.AnswerRepository
                .GetAllAsync(a => !a.IsDeleted && a.CreatedBy == User.Identity.Name, new[] { "Question" }); 

            return View(answers);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Answer answer)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns();
                return View(answer);
            }

            var existingAnswer = await unitOfWork.AnswerRepository
                .GetAsync(a => a.AnswerText == answer.AnswerText && a.QuestionId == answer.QuestionId && a.CreatedBy == User.Identity.Name);

            if (existingAnswer != null)
            {
                if (existingAnswer.IsDeleted)
                {
                    existingAnswer.IsDeleted = false;
                    existingAnswer.IsCorrect = answer.IsCorrect;
                    existingAnswer.LastModifiedAt = DateTime.UtcNow;
                    existingAnswer.LastModifiedBy = User.Identity?.Name ?? "System";

                    unitOfWork.AnswerRepository.Update(existingAnswer);
                    await unitOfWork.CompleteAsync();

                    return RedirectToAction(nameof(GetAll));
                }

                ModelState.AddModelError("AnswerText", "This answer already exists for the selected question.");
                PopulateDropdowns();
                return View(answer);
            }

            answer.CreatedAt = DateTime.UtcNow;
            answer.CreatedBy = User.Identity?.Name ?? "System";

            await unitOfWork.AnswerRepository.AddAsync(answer);
            await unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(GetAll));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var answer = await unitOfWork.AnswerRepository.GetAsync(a => a.ID == id && a.CreatedBy == User.Identity.Name, new[] { "Question" });

            if (answer == null)
            {
                TempData["Error"] = "Answer not found!";
                return RedirectToAction(nameof(GetAll));
            }

            PopulateDropdowns();
            return View(answer);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Answer answer)
        {
            if (id != answer.ID)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                PopulateDropdowns();
                return View(answer);
            }

            var existingAnswer = await unitOfWork.AnswerRepository.GetAsync(a => a.ID == id && a.CreatedBy == User.Identity.Name);
            if (existingAnswer == null)
            {
                TempData["Error"] = "Answer not found!";
                return RedirectToAction(nameof(GetAll));
            }

            existingAnswer.AnswerText = answer.AnswerText;
            existingAnswer.IsCorrect = answer.IsCorrect;
            existingAnswer.QuestionId = answer.QuestionId;
            existingAnswer.LastModifiedAt = DateTime.UtcNow;
            existingAnswer.LastModifiedBy = User.Identity.Name ?? "System";

            unitOfWork.AnswerRepository.Update(existingAnswer);
            await unitOfWork.CompleteAsync();

            TempData["Success"] = "Answer updated successfully!";
            return RedirectToAction(nameof(GetAll));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var answer = await unitOfWork.AnswerRepository
                .GetAsync(a => a.ID == id && a.CreatedBy == User.Identity.Name);

            if (answer == null || answer.IsDeleted)
            {
                TempData["Error"] = "Answer not found or already deleted.";
                return Json(new { success = false });
            }
            

            answer.IsDeleted = true;
            await unitOfWork.CompleteAsync();

            TempData["Success"] = "Answer Deleted Successfully";
            return Json(new { success = true });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var answer = await unitOfWork.AnswerRepository
                .GetAsync(a => !a.IsDeleted && a.ID == id && a.CreatedBy == User.Identity.Name, new[] { "Question" });

            if (answer == null)
            {
                TempData["Error"] = "No Answer found with the provided ID.";
                return RedirectToAction(nameof(GetAll));
            }

            return View(answer);
        }


        private void PopulateDropdowns()
        {
            ViewBag.Questions = new SelectList(unitOfWork.QuestionRepository
                .GetAllAsync(q => !q.IsDeleted && q.CreatedBy == User.Identity.Name).Result, "ID", "QuestionText");
        }



    }
}
