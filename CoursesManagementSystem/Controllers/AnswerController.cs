using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.Repository;
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


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var answers = await unitOfWork.AnswerRepository
                .GetAllAsync(a => !a.IsDeleted, new[] { "Question" }); 

            return View(answers);
        }


        [HttpGet]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }


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
                .GetAsync(a => a.AnswerText == answer.AnswerText && a.QuestionId == answer.QuestionId);

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


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var answer = await unitOfWork.AnswerRepository.GetAsync(a => a.ID == id, new[] { "Question" });

            if (answer == null)
            {
                TempData["Error"] = "Answer not found!";
                return RedirectToAction(nameof(GetAll));
            }

            PopulateDropdowns();
            return View(answer);
        }


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

            var existingAnswer = await unitOfWork.AnswerRepository.GetAsync(a => a.ID == id);
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


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var answer = await unitOfWork.AnswerRepository
                .GetAsync(a => !a.IsDeleted && a.ID == id, new[] { "Question" });

            if (answer == null)
            {
                TempData["Error"] = "No Answer found with the provided ID.";
                return RedirectToAction(nameof(GetAll));
            }

            return View(answer);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var answer = await unitOfWork.AnswerRepository
                .GetAsync(a => a.ID == id);

            if (answer == null || answer.IsDeleted)
            {
                TempData["Error"] = "Answer not found or already deleted.";
                return RedirectToAction(nameof(GetAll));
            }

            answer.IsDeleted = true;
            await unitOfWork.CompleteAsync();

            TempData["Success"] = "Answer deleted successfully.";
            return RedirectToAction(nameof(GetAll));
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var answer = await unitOfWork.AnswerRepository
                .GetAsync(a => !a.IsDeleted && a.ID == id, new[] { "Question" });

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
                .GetAllAsync(q => !q.IsDeleted).Result, "ID", "QuestionText");
        }



    }
}
