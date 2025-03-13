using AutoMapper;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Enums;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
                        new string[] { "Lesson", "Lesson.Chapter", "Lesson.Chapter.Course", "QuestionLevel" }
                      );


            foreach (var question in questions)
                {
                    if (question.Lesson != null && question.Lesson.Chapter.Course != null)
                    {
                        question.Lesson.Name = $"{question.Lesson.Name} - {question.Lesson.Chapter.Course.Name}";
                    }
                }

                return View(questions);
            }

            [HttpGet]
            public async Task<IActionResult> Create()
            {
                await PopulateDropdowns();
                return View();
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question, List<string> Answers, string CorrectAnswer)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
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
                await PopulateDropdowns();
                return View(question);
            }

            question.CreatedAt = DateTime.UtcNow;
            question.CreatedBy = User.Identity?.Name ?? "System";

            await unitOfWork.QuestionRepository.AddAsync(question);
            await unitOfWork.CompleteAsync(); // Save question to get ID

            // ✅ Fix: Use Enum Comparison Correctly
            if (question.QuestionType == (QuestionTypeEnum)1) // Multiple Choice
            {
                for (int i = 0; i < Answers.Count; i++)
                {
                    var answer = new Answer
                    {
                        QuestionId = question.ID,
                        AnswerText = Answers[i],
                        IsCorrect = (CorrectAnswer == (i + 1).ToString())
                    };
                    await unitOfWork.AnswerRepository.AddAsync(answer);
                }
            }
            else if (question.QuestionType == (QuestionTypeEnum)2) // True/False
            {
                await unitOfWork.AnswerRepository.AddAsync(new Answer
                {
                    QuestionId = question.ID,
                    AnswerText = "True",
                    IsCorrect = (CorrectAnswer == "true")
                });

                await unitOfWork.AnswerRepository.AddAsync(new Answer
                {
                    QuestionId = question.ID,
                    AnswerText = "False",
                    IsCorrect = (CorrectAnswer == "false")
                });
            }

            await unitOfWork.CompleteAsync(); // Save answers

            TempData["Success"] = "Question and answers added successfully!";
            return RedirectToAction(nameof(GetAll));
        }



        [HttpGet]
            public async Task<IActionResult> Edit(int id)
            {
                var question = await unitOfWork.QuestionRepository.GetAsync(q => q.ID == id, new[] { "Lesson", "QuestionLevel" });

                if (question == null)
                {
                    TempData["Error"] = "Question not found!";
                    return RedirectToAction(nameof(GetAll));
                }

                await PopulateDropdowns();
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
                    await PopulateDropdowns();
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
                existingQuestion.LastModifiedBy = User.Identity?.Name ?? "System";

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
                var question = await unitOfWork.QuestionRepository.GetAsync(q => q.ID == id);

                if (question == null || question.IsDeleted)
                {
                    TempData["Error"] = "Question not found or already deleted.";
                    return RedirectToAction(nameof(GetAll));
                }

            var QuestionWithAnswerfound = await unitOfWork.AnswerRepository.GetAsync(l => !l.IsDeleted && l.QuestionId == id);

            if (QuestionWithAnswerfound != null)
            {
                //return BadRequest();
                TempData["Error"] = "Can not delete a Question having Answer";
                return RedirectToAction("Index");
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

            private async Task PopulateDropdowns()
            {
            var lessons = await unitOfWork.LessonRepository
               .GetAllAsync(l => !l.IsDeleted, new string[] { "Chapter", "Chapter.Course" });

            ViewBag.Lessons = lessons.Select(l => new SelectListItem
            {
                Value = l.ID.ToString(),
                Text = $"{l.Name} - {l.Chapter?.Course?.Name}"
            });

            ViewBag.QuestionLevels = (await unitOfWork.QuestionLevelRepository.GetAllAsync(q => !q.IsDeleted))
                    .Select(q => new SelectListItem
                    {
                        Value = q.ID.ToString(),
                        Text = q.Name
                    });

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


        /*
        
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


      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await unitOfWork.QuestionRepository
                .GetAsync(q => q.ID == id);

            if (question == null || question.IsDeleted)
            {
                TempData["Error"] = "Question not found or already deleted.";
                return Json(new { success =false });
            }

            question.IsDeleted = true;

            await unitOfWork.CompleteAsync();

            TempData["Success"] = "Question deleted successfully.";
            return Json(new { success = true });
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
*/

 
