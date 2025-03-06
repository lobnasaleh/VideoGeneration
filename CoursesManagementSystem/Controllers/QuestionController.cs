using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Enums;
using CoursesManagementSystem.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoursesManagementSystem.Controllers
{
    public class QuestionController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public QuestionController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var questions = await unitOfWork.QuestionRepository.GetAllAsync(
                c => !c.IsDeleted,
                new string[] { "Lesson", "QuestionLevel" }
            );

            return View(questions);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Question question)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        PopulateDropdowns();
        //        return View(question);
        //    }

        //    var existingQuestion = await unitOfWork.QuestionRepository
        //        .GetAsync(q => q.QuestionText == question.QuestionText && q.LessonId == question.LessonId);

        //    if (existingQuestion != null)
        //    {
        //        ModelState.AddModelError("QuestionText", "This question already exists for the selected lesson.");
        //        PopulateDropdowns();
        //        return View(question);
        //    }

        //    question.CreatedAt = DateTime.UtcNow;
        //    question.CreatedBy = User.Identity?.Name ?? "System";

        //    await unitOfWork.QuestionRepository.AddAsync(question);
        //    await unitOfWork.CompleteAsync();

        //    return RedirectToAction(nameof(GetAll));
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns();
                return View(question);
            }

            var existingQuestion = await unitOfWork.QuestionRepository
                .GetAsync(q => q.QuestionText == question.QuestionText && q.LessonId == question.LessonId);

            if (existingQuestion != null)
            {
                if (existingQuestion.IsDeleted)
                {
                    existingQuestion.IsDeleted = false;
                    existingQuestion.LastModifiedAt = DateTime.UtcNow;
                    existingQuestion.LastModifiedBy = User.Identity?.Name ?? "System";

                    unitOfWork.QuestionRepository.Update(existingQuestion);
                    await unitOfWork.CompleteAsync();

                    TempData["Success"] = "Question restored successfully!";
                    return RedirectToAction(nameof(GetAll));
                }

                ModelState.AddModelError("QuestionText", "This question already exists for the selected lesson.");
                PopulateDropdowns();
                return View(question);
            }

            question.CreatedAt = DateTime.UtcNow;
            question.CreatedBy = User.Identity?.Name ?? "System";

            await unitOfWork.QuestionRepository.AddAsync(question);
            await unitOfWork.CompleteAsync();

            TempData["Success"] = "Question added successfully!";
            return RedirectToAction(nameof(GetAll));
        }



        [HttpGet]
        public IActionResult Edit(int id)
        {
            var question = unitOfWork.QuestionRepository.GetAsync(q => q.ID == id, new[] { "Lesson", "QuestionLevel" }).Result;

            if (question == null)
            {
                TempData["Error"] = "Question not found!";
                return RedirectToAction(nameof(GetAll));
            }

            PopulateDropdowns();

            return View(question);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Question question)
        {
            if (id != question.ID)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                PopulateDropdowns();
                return View(question);
            }

            var existingQuestion = await unitOfWork.QuestionRepository.GetAsync(q => q.ID == id);
            if (existingQuestion == null)
            {
                TempData["Error"] = "Question not found!";
                return RedirectToAction(nameof(GetAll));
            }

            existingQuestion.QuestionText = question.QuestionText;
            existingQuestion.QuestionInstructions = question.QuestionInstructions;
            existingQuestion.LessonId = question.LessonId;
            existingQuestion.QuestionLevelId = question.QuestionLevelId;
            existingQuestion.QuestionType = question.QuestionType;
            existingQuestion.LastModifiedAt = DateTime.UtcNow;
            existingQuestion.LastModifiedBy = User.Identity.Name ?? "System";

            unitOfWork.QuestionRepository.Update(existingQuestion);
            await unitOfWork.CompleteAsync();

            TempData["Success"] = "Question updated successfully!";
            return RedirectToAction(nameof(GetAll));
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await unitOfWork.QuestionRepository
                .GetAsync(q => !q.IsDeleted && q.ID == id, new[] { "Lesson", "QuestionLevel" });

            if (question == null)
            {
                TempData["Error"] = "No Question found with the provided ID.";
                return RedirectToAction(nameof(GetAll));
            }

            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await unitOfWork.QuestionRepository
                .GetAsync(q => q.ID == id);

            if (question == null || question.IsDeleted)
            {
                TempData["Error"] = "Question not found or already deleted.";
                return RedirectToAction(nameof(GetAll));
            }

            question.IsDeleted = true;

            await unitOfWork.CompleteAsync();

            TempData["Success"] = "Question deleted successfully.";
            return RedirectToAction(nameof(GetAll));
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var question = await unitOfWork.QuestionRepository
                .GetAsync(q => !q.IsDeleted && q.ID == id, new[] { "Lesson", "QuestionLevel" });

            if (question == null)
            {
                TempData["Error"] = "No Question found with the provided ID.";
                return RedirectToAction(nameof(GetAll));
            }

            return View(question);
        }


        private void PopulateDropdowns()
        {
            ViewBag.Lessons = new SelectList(
                unitOfWork.LessonRepository.GetAllAsync(l => !l.IsDeleted).Result, "ID", "Name"
            );

            ViewBag.QuestionLevels = new SelectList(
                unitOfWork.QuestionLevelRepository.GetAllAsync(q => !q.IsDeleted).Result, "ID", "Name"
            );

            ViewBag.QuestionTypes = Enum.GetValues(typeof(QuestionTypeEnum))
                .Cast<QuestionTypeEnum>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                });
        }


    }
}
