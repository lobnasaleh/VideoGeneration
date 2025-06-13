using AutoMapper;
using Azure;
using CoursesManagementSystem.Constants;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.DTOs;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.Repository;
using CoursesManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace CoursesManagementSystem.Controllers
{
    public class CourseController : Controller
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly string _BookPath;
        private readonly string _ImagePath;

        private readonly IHttpClientFactory _httpClientFactory;
        public CourseController(IMapper _mapper, IUnitOfWork _unitOfWork, IWebHostEnvironment webHostEnvironment ,IHttpClientFactory httpClientFactory)
        {

            _httpClientFactory = httpClientFactory;
            mapper = _mapper;
            unitOfWork = _unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
            _BookPath = $"{webHostEnvironment.WebRootPath}{UploadsSettings.BooksPath}";
            _ImagePath= $"{webHostEnvironment.WebRootPath}{UploadsSettings.ImagesPath}";

            if (!Directory.Exists(_BookPath))
            {
                Directory.CreateDirectory(_BookPath);
            }

            if (!Directory.Exists(_ImagePath))
            {
                Directory.CreateDirectory(_ImagePath);
                Console.WriteLine("Directory created successfully.");
            }
            else
            {
                Console.WriteLine("Directory already exists.");
            }
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {

            List<CourseVM> res = await unitOfWork.CourseRepository.GetAllQuery(a => !a.IsDeleted && a.CreatedBy==User.Identity.Name)
                .Select(c => new CourseVM
                {
                    Id = c.ID,
                    LevelName = c.Level.Name,
                    CategoryName = c.Category.Name,
                    BookStorageURL = c.BookStorageURL,//--->>
                    Name = c.Name,
                    Details = c.Details,
                    LevelId = c.LevelId,
                    CategoryId = c.CategoryId,
                })
                .ToListAsync();
            // var res= await unitOfWork.CourseRepository.GetAllAsync(c=>!c.IsDeleted,include: new[] {"Level","Category"} );

            return View(res);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            CourseVM catLvl = new CourseVM()
            {
                Categories = await unitOfWork.CategoryRepository.GetAllAsync(c=>!c.IsDeleted ),
                Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted)
            };

            return View(catLvl);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CourseVM courseVM)
        {
           
            if (ModelState.IsValid)
            {
                //check if course name is unique

                Course coursefound = await unitOfWork.CourseRepository.GetAsync(c => !c.IsDeleted && c.Name == courseVM.Name && c.CreatedBy == User.Identity.Name);
                if (coursefound != null)
                {
                    ModelState.AddModelError("Name", "There is already a Course with This Name");

                    //refill selects

                    courseVM.Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted);
                    courseVM.Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted);

                    return View(courseVM);
                }
                //check if level is marked deleted -->mark undeleted
                var deletedCourse = await unitOfWork.CourseRepository
                    .GetAsync(c => c.IsDeleted
                   && c.Details == courseVM.Details && c.Name == courseVM.Name
                    && c.CategoryId == courseVM.CategoryId && c.LevelId == courseVM.LevelId
                   && c.BookStorageURL == courseVM.BookStorageURL
                    && c.CreatedBy == User.Identity.Name
                    );
                if (deletedCourse != null)
                {
                    deletedCourse.IsDeleted = false;
                    await unitOfWork.CompleteAsync();
                    return RedirectToAction("Index");

                }

                if (courseVM.Book != null && courseVM.Book.Length > 0)
                {
                    // Validate file type
                    var allowedBookExtensions = new[] { ".pdf", ".doc", ".docx" };
                    var bookExtension = Path.GetExtension(courseVM.Book.FileName).ToLower();

                    if (!allowedBookExtensions.Contains(bookExtension))
                    {
                        ModelState.AddModelError("Book", "Only PDF, DOC, and DOCX files are allowed for books.");
                        courseVM.Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted );
                        courseVM.Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted);
                        return View(courseVM);
                    }

                   /* // Validate file size (1 MB = 1024 * 1024 bytes)
                    if (courseVM.Book.Length > 1024 * 1024)
                    {
                        ModelState.AddModelError("Book", "Book file size must not exceed 1 MB.");
                        courseVM.Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted);
                        courseVM.Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted);
                        return View(courseVM);
                    }*/
                }

                // Validate CourseImage upload
                if (courseVM.CourseImage != null && courseVM.CourseImage.Length > 0)
                {
                    // Validate file type
                    var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png" , ".gif", };
                    var imageExtension = Path.GetExtension(courseVM.CourseImage.FileName).ToLower();

                    if (!allowedImageExtensions.Contains(imageExtension))
                    {
                        ModelState.AddModelError("CourseImage", "Only JPEG, JPG, GIF and PNG files are allowed for images.");
                        courseVM.Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted);
                        courseVM.Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted );
                        return View(courseVM);
                    }

                    // Validate file size (1 MB = 1024 * 1024 bytes)
                   /* if (courseVM.CourseImage.Length > 1024 * 1024)
                    {
                        ModelState.AddModelError("CourseImage", "Image file size must not exceed 1 MB.");
                        courseVM.Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted);
                        courseVM.Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted);
                        return View(courseVM);

                    }*/
                }

                string bookStorageUrl = await ProcessFileUpload(courseVM.Book, _BookPath, UploadsSettings.BooksPath);
                if (bookStorageUrl == null)
                {
                    ModelState.AddModelError("", "Book upload failed.");
                    courseVM.Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted );
                    courseVM.Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted);

                    return View(courseVM);
                }
                string imageStorageUrl = await ProcessFileUpload(courseVM.CourseImage, _ImagePath, UploadsSettings.ImagesPath);
                if (imageStorageUrl == null)
                {
                    ModelState.AddModelError("", "Image upload failed.");
                    courseVM.Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted);
                    courseVM.Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted);

                    return View(courseVM);
                }

                //new Course
                     Course cmp = mapper.Map<Course>(courseVM);
                     cmp.CreatedAt = DateTime.Now;
                     cmp.CreatedBy=User.Identity.Name;
                     cmp.BookStorageURL = bookStorageUrl;// Store relative path
                     cmp.CourseImageStorageURL = imageStorageUrl;// Store relative path
                await unitOfWork.CourseRepository.AddAsync(cmp);
                     await unitOfWork.CompleteAsync();
                     return RedirectToAction("Index");

            }
            //refill selects

            courseVM.Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted );
            courseVM.Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted);

            return View(courseVM);

        }

        private async Task<string> ProcessFileUpload(IFormFile file, string basePath, string relativePath)
        {

            if (file==null || file.Length == 0)
            {
                return null;
            }
            try
            {
                var FileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var path = Path.Combine(basePath, FileName);

                // Save the file to the server
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return $"{relativePath}/{FileName}";
            }
            catch(Exception ex)
            {
                return null;
            }


        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Update(int id)
        {
            var Course = await unitOfWork.CourseRepository.GetByIdAsync(id);
            if (Course == null)
            {
                TempData["Error"] = "No Course with This Id is Found";
                return RedirectToAction("Index");
            }
            UpdateCourseVM res = mapper.Map<UpdateCourseVM>(Course);

            var Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted ) ?? new List<Category>();
            var Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted) ?? new List<Level>();
            ViewBag.categorySelectList = GetCategorySelectList(Categories);
            ViewBag.levelSelectList = GetLevelSelectList(Levels);
            return View(res);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Update(int id, UpdateCourseVM CourseVM)
        {
            CourseVM ??= new UpdateCourseVM();
            var Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted) ?? new List<Category>();
            var Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted) ?? new List<Level>();
            if (ModelState.IsValid)
            {

                Course lv = await unitOfWork.CourseRepository.GetByIdAsync(id);
                if (lv == null)
                {
                    TempData["Error"] = "No Course with This Id is Found";
                    return RedirectToAction("Index");
                }
                //check if it is unique
                var Course = await unitOfWork.CourseRepository
                    .GetAsync(l => !l.IsDeleted && l.Name == CourseVM.Name && l.ID!=id && l.CreatedBy == User.Identity.Name, null, false);
                //en el id mokhtalef ma3anh enha msh ely howa byhawel ye3mlha update halyan ,laken law el esm howa howa fel submit matghyrash yb2a 3ady
                if (Course != null)
                {
                    ModelState.AddModelError("Name", "Course Name already exists ");
                    ViewBag.categorySelectList = GetCategorySelectList(Categories);
                    ViewBag.levelSelectList = GetLevelSelectList(Levels);
                    return View(CourseVM);
                }

                //check if Course is marked deleted -->mark undeleted
                var deletedCourse = await unitOfWork.CourseRepository.GetAsync(l => l.IsDeleted && l.Name == CourseVM.Name && l.CreatedBy == User.Identity.Name);
                if (deletedCourse != null)
                {
                    lv.IsDeleted = true;
                    deletedCourse.IsDeleted = false;
                    await unitOfWork.CompleteAsync();
                    return RedirectToAction("Index");

                }

                if (CourseVM.Book != null && CourseVM.Book.Length > 0)
                    {

                        // Delete the old file if it exists
                        if (!string.IsNullOrEmpty(lv.BookStorageURL))
                        {
                            var oldFilePath = Path.Combine(webHostEnvironment.WebRootPath, lv.BookStorageURL.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                    // Process the new book file upload
                    string bookRelativePath = await ProcessFileUpload(
                        CourseVM.Book,
                        _BookPath,
                        UploadsSettings.BooksPath
                    );

                    if (bookRelativePath == null)
                    {
                        ModelState.AddModelError("", "Failed to upload the book file.");
                        ViewBag.categorySelectList = GetCategorySelectList(Categories);
                        ViewBag.levelSelectList = GetLevelSelectList(Levels);
                        return View(CourseVM); // Return to the view with validation errors
                    }


                        // Update the file path
                        lv.BookStorageURL = bookRelativePath;
                }
                if (CourseVM.CourseImage != null && CourseVM.CourseImage.Length > 0)
                {

                    // Delete the old file if it exists
                    if (!string.IsNullOrEmpty(lv.CourseImageStorageURL))
                    {
                        var oldFilePath2 = Path.Combine(webHostEnvironment.WebRootPath, lv.CourseImageStorageURL.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath2))
                        {
                            System.IO.File.Delete(oldFilePath2);
                        }
                    }

                    // Process the new book file upload
                    string ImageRelativePath = await ProcessFileUpload(
                        CourseVM.CourseImage,
                        _ImagePath,
                        UploadsSettings.ImagesPath 
                    );

                    if (ImageRelativePath == null)
                    {
                        ModelState.AddModelError("", "Failed to upload the Image");
                        ViewBag.categorySelectList = GetCategorySelectList(Categories);
                        ViewBag.levelSelectList = GetLevelSelectList(Levels);
                        return View(CourseVM); // Return to the view with validation errors
                    }


                    // Update the file path
                    lv.CourseImageStorageURL = ImageRelativePath;
                }

                lv.LastModifiedAt = DateTime.Now;
                lv.Name = CourseVM.Name;
                lv.Details = CourseVM.Details;
                lv.CategoryId = CourseVM.CategoryId;
                lv.LevelId = CourseVM.LevelId;

                lv.LastModifiedBy = User.Identity.Name ?? "System";
                await unitOfWork.CompleteAsync();
                return RedirectToAction("Index");

            }
               

            ViewBag.categorySelectList = GetCategorySelectList(Categories);
            ViewBag.levelSelectList = GetLevelSelectList(Levels);


            return View(CourseVM);

        }
        private List<SelectListItem> GetCategorySelectList(IEnumerable<Category> categories)
        {
            return categories.Select(c => new SelectListItem
            {
                Value = c.ID.ToString(), 
                Text = c.Name
            }).ToList();
        }

        private List<SelectListItem> GetLevelSelectList(IEnumerable<Level> levels)
        {
            return levels.Select(l => new SelectListItem
            {
                Value = l.ID.ToString(),
                Text = l.Name
            }).ToList();
        }



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> getById(int id)
        {
            CourseDetailsVM l = await unitOfWork.CourseRepository.GetQuery(c => !c.IsDeleted && c.ID == id && c.CreatedBy == User.Identity.Name)
                .Select(c => new CourseDetailsVM
                {
                    CategoryName = c.Category.Name,
                    LevelName = c.Level.Name,
                    Name = c.Name,
                    Details = c.Details,
                    BookStorageURL = c.BookStorageURL,
                    CategoryId = c.CategoryId,
                    LevelId = c.LevelId,
                    Id = c.ID,
                    LastModifiedAt = c.LastModifiedAt,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.CreatedBy,
                    LastModifiedBy = c.LastModifiedBy
                    

                })
                .FirstOrDefaultAsync();

            if (l == null)
            {
                // return NotFound();

                TempData["Error"] = "No Course with This Id is Found";
                return RedirectToAction("Index");
            }
            return View(l);
        }
     
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            Course l = await unitOfWork.CourseRepository.GetAsync(l => !l.IsDeleted && l.ID == id && l.CreatedBy == User.Identity.Name);
            if (l == null)
            {
                //return NotFound();

                TempData["Error"] = "No Course with this Id is Found";
                return Json(new { success = false });
            }
            //check if Course is not assigned to a courseconfig,coursequestionconfig,chapter
            var coursewithChapterfound = await unitOfWork.ChapterRepository.GetAsync(c => !c.IsDeleted && c.CourseId == id && c.CreatedBy == User.Identity.Name) ;
            var coursewithCourseConfigfound = await unitOfWork.CourseConfigRepository.GetAsync(cc => !cc.IsDeleted && cc.CourseId == id && cc.CreatedBy == User.Identity.Name);
            var coursewithCourseQuestionConfigfound = await unitOfWork.CourseQuestionConfigRepository.GetAsync(cq => !cq.IsDeleted && cq.CourseId == id && cq.CreatedBy == User.Identity.Name);

            if (coursewithChapterfound != null)
            {
                //return BadRequest();
                TempData["Error"] = "Can not delete a Course that is assigned to a Chapter";
                return Json(new { success = false });
            }
            if (coursewithCourseConfigfound != null || coursewithCourseQuestionConfigfound != null)
            {
                //return BadRequest();
                TempData["Error"] = "Can not delete a Course that has Configurations";
                return Json(new { success = false });
            }
            // Delete the file if it exists
            if (!string.IsNullOrEmpty(l.BookStorageURL))
            {
                var oldFilePath = Path.Combine(webHostEnvironment.WebRootPath, l.BookStorageURL.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            if (!string.IsNullOrEmpty(l.CourseImageStorageURL))
            {
                var oldFilePath = Path.Combine(webHostEnvironment.WebRootPath, l.CourseImageStorageURL.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            l.IsDeleted = true;
            await unitOfWork.CompleteAsync();
            TempData["Success"] = "Course Deleted Successfully";
            return Json(new { success = true });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> getGeneratedCourseById(int id)
        {

            var course = await unitOfWork.CourseRepository.GetAsync(c => !c.IsDeleted && c.ID == id && c.CreatedBy == User.Identity.Name,
            new[] { "Category", "Level", "CourseConfig", "CourseQuestionsConfig.QuestionLevel" }
        );
            if (course == null)
            {
                TempData["Error"] = "No Course with This Id is Found";
                return RedirectToAction("getGeneratedCourses");//getGeneratedCoursees
            }


            var chapters = await unitOfWork.ChapterRepository.GetAllQuery(ch => !ch.IsDeleted && ch.CourseId == id && ch.CreatedBy == User.Identity.Name)
                .ToListAsync();
            var chapterIds = chapters.Select(ch => ch.ID).ToList();
            var lessons = await unitOfWork.LessonRepository.GetAllQuery(l => !l.IsDeleted && chapterIds.Contains(l.ChapterId) && l.CreatedBy == User.Identity.Name)
                .ToListAsync();

            var res = new GeneratedCourseVM
            {
                CourseImage=course.CourseImageStorageURL,
                BookStorageURL = course.BookStorageURL,
                CategoryId = course.CategoryId,
                CategoryName = course.Category?.Name ?? "Unknown",
                Name = course.Name,
                CourseId = course.ID,
                Details = course.Details,
                FirstGeneratedLessonOfChapterOneID = chapters
                                              .Where(ch => ch.CourseId == id)
                                              .OrderBy(ch => ch.ID)
                                              .Take(1)
                                             .SelectMany(ch => lessons
                                             .Where(l => l.ChapterId == ch.ID)
                                             .OrderBy(l => l.ID)
                                             .Take(1)
                                             .Select(l => l.ID))
                                            .FirstOrDefault(),
                LevelId = course.LevelId,
                LevelName = course.Level?.Name ?? "Unknown",
                ChaptersCount = course.CourseConfig?.ChaptersCount ?? 0,
                LessonsCountPerChapter = course.CourseConfig?.LessonsCountPerChapter ?? 0,
                VideoDurationInMin = course.CourseConfig?.VideoDurationInMin ?? 0,
                TotalCourseDuration =
    (double?)course.CourseConfig?.ChaptersCount *
    course.CourseConfig?.LessonsCountPerChapter *
    course.CourseConfig?.VideoDurationInMin / 60 ?? 20,

                //TotalCourseDuration = (course.CourseConfig?.ChaptersCount * course.CourseConfig?.LessonsCountPerChapter * course.CourseConfig?.VideoDurationInMin )/60?? 20,
                Language = course.CourseConfig?.Language.ToString() ?? "Unknown",
                Persona = course.CourseConfig?.Persona.ToString() ?? "Unknown",
                Chapters = chapters
                    .Where(ch => ch.CourseId == course.ID)
                    .Select(ch => new GeneratedChapterVM
                    {
                        ChapterId = ch.ID,
                        Name = ch.Name,
                        Details = ch.Details,
                        Sort = ch.Sort,
                        Lessons = lessons
                            .Where(l => l.ChapterId == ch.ID)
                            .Select(l => new GeneratedLessonVM
                            {
                                LessonId = l.ID,
                                Name = l.Name,
                                Details = l.Details,
                                ScriptText = l.ScriptText,
                                VideoStorageURL = l.VideoStorageURL,
                                AudioStorageURL = l.AudioStorageURL,
                                Sort = l.Sort
                            }).ToList(),
                        LessonsCount = lessons.Count(ll => ll.ChapterId == ch.ID)
                    }).ToList(),
                CourseQuestionConfig = course.CourseQuestionsConfig
                    .Where(cq => !cq.IsDeleted)
                    .Select(cqq => new GeneratedCourseQuestionConfigVM
                    {
                        QuestionLevelId = cqq.QuestionLevelId,
                        QuestionLevelName = cqq.QuestionLevel?.Name ?? "Unknown",
                        QuestionsCountPerLesson = cqq.QuestionsCountPerLesson
                    }).ToList()
            };

            if (res.FirstGeneratedLessonOfChapterOneID == 0) //3shan ma yb3tash el lessonid b 0 lama yeegy yeshoof el curriculum w ykoon fady
            {
                TempData["Error"] = "Lessons are not yet generated ,Hold On!";
                return RedirectToAction("getGeneratedCourses");
            }
            return View(res);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> getGeneratedCourses()
        {

            var courses = await unitOfWork.CourseRepository.GetAllAsync(c => !c.IsDeleted && c.CreatedBy == User.Identity.Name,
            new[] { "Category", "Level", "CourseConfig", "CourseQuestionsConfig.QuestionLevel" }
        );
            if (!courses.Any())
            {
                TempData["Error"] = "No Courses Found,Start by generating a new one!";
                return RedirectToAction("Index");
            }


            var chapters = await unitOfWork.ChapterRepository.GetAllQuery(ch => !ch.IsDeleted && ch.CreatedBy == User.Identity.Name)
                .ToListAsync();
            var chapterIds = chapters.Select(ch => ch.ID).ToList();
            var lessons = await unitOfWork.LessonRepository.GetAllQuery(l => !l.IsDeleted && l.CreatedBy == User.Identity.Name && chapterIds.Contains(l.ChapterId))
                .ToListAsync();

            var res = courses.Select(c => new GeneratedCourseVM
            {
                CourseImage = c.CourseImageStorageURL,
                BookStorageURL = c.BookStorageURL,
                CategoryId = c.CategoryId,
                CategoryName = c.Category?.Name ?? "Unknown",
                Name = c.Name,
                CourseId = c.ID,
                Details = c.Details,
                LevelId = c.LevelId,
                LevelName = c.Level?.Name ?? "Unknown",
                ChaptersCount = c.CourseConfig?.ChaptersCount ?? 0,
                LessonsCountPerChapter = c.CourseConfig?.LessonsCountPerChapter ?? 0,
                VideoDurationInMin = c.CourseConfig?.VideoDurationInMin ?? 0,
                Language = c.CourseConfig?.Language.ToString() ?? "Unknown",
                Persona = c.CourseConfig?.Persona.ToString() ?? "Unknown",
                Chapters = chapters
                        .Where(ch => ch.CourseId == c.ID)
                        .Select(ch => new GeneratedChapterVM
                        {
                            ChapterId = ch.ID,
                            Name = ch.Name,
                            Details = ch.Details,
                            Sort = ch.Sort,
                            Lessons = lessons
                                .Where(l => l.ChapterId == ch.ID)
                                .Select(l => new GeneratedLessonVM
                                {
                                    LessonId = l.ID,
                                    Name = l.Name,
                                    Details = l.Details,
                                    ScriptText = l.ScriptText,
                                    VideoStorageURL = l.VideoStorageURL,
                                    AudioStorageURL = l.AudioStorageURL,
                                    Sort = l.Sort
                                }).ToList(),
                            LessonsCount = lessons.Count(ll => ll.ChapterId == ch.ID)
                        }).ToList(),
                CourseQuestionConfig = c.CourseQuestionsConfig
                        .Where(cq => !cq.IsDeleted)
                        .Select(cqq => new GeneratedCourseQuestionConfigVM
                        {
                            QuestionLevelId = cqq.QuestionLevelId,
                            QuestionLevelName = cqq.QuestionLevel?.Name ?? "Unknown",
                            QuestionsCountPerLesson = cqq.QuestionsCountPerLesson
                        }).ToList()
            }).ToList();
            return View(res);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewLesson(int id)
        {
            var lesson = await unitOfWork.LessonRepository.GetLessonWithQuestionsAndAnswersAsync(id);
            if (lesson == null)
                return NotFound();

            var allLessonsInCourse = await unitOfWork.LessonRepository
                .GetLessonsByCourseIdAsync(lesson.Chapter.CourseId);

            var orderedLessons = allLessonsInCourse
                .OrderBy(l => l.ChapterId)
                .ThenBy(l => l.ID) 
                .ToList();

            var currentIndex = orderedLessons.FindIndex(l => l.ID == id);

            int? nextLessonId = null;
            if (currentIndex >= 0 && currentIndex < orderedLessons.Count - 1)
            {
                nextLessonId = orderedLessons[currentIndex + 1].ID;
            }


            var viewModel = new LessonDetailViewModel
            {
                LessonId = lesson.ID,
                Name = lesson.Name,
                Details = lesson.Details,
                ScriptText = lesson.ScriptText,
                VideoStorageURL = lesson.VideoStorageURL,
                ChapterName = lesson.Chapter?.Name,
                CourseId = lesson.Chapter.CourseId,
                NextLessonId = nextLessonId,

                Questions = lesson.Questions?.Select(q => new QuestionViewModel
                {
                    QuestionId = q.ID,
                    QuestionText = q.QuestionText,
                    QuestionInstructions = q.QuestionInstructions,
                    QuestionType = q.QuestionType.ToString(),
                    Answers = q.Answers?.Select(a => new AnswerViewModel
                    {
                        AnswerId = a.ID,
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        //[HttpPost]
        //public async Task<IActionResult> SendCourseToAI(int courseId)
        //{
        //    var course = await unitOfWork.CourseRepository.GetCourseWithConfigsAsync(courseId);
        //    if (course == null)
        //    {
        //        TempData["Error"] = "Course not found.";
        //        return RedirectToAction("Details", new { id = courseId });
        //    }

        //    Console.WriteLine("=== DEBUG: Raw Course Data ===");
        //    Console.WriteLine($"CourseId: {course.ID}");
        //    Console.WriteLine($"ChaptersCount: {course.CourseConfig?.ChaptersCount ?? -1}");
        //    Console.WriteLine($"Language: {course.CourseConfig?.Language.ToString() ?? "NULL"}");
        //    Console.WriteLine($"Persona: {course.CourseConfig?.Persona.ToString() ?? "NULL"}");


        //    if (course.CourseQuestionsConfig != null)
        //    {
        //        foreach (var cfg in course.CourseQuestionsConfig)
        //        {
        //            Console.WriteLine($"  - Q Count: {cfg.QuestionsCountPerLesson}, LevelId: {cfg.QuestionLevelId}, LevelName: {cfg.QuestionLevel?.Name}");
        //        }
        //    }


        //    var courseDto = mapper.Map<CourseGenerationDTO>(course);

        //    var httpClient = _httpClientFactory.CreateClient();

        //    var json = JsonSerializer.Serialize(courseDto);
        //    var content = new StringContent(json, Encoding.UTF8, "application/json");

        //    var response = await httpClient.PostAsync("https://httpbin.org/post", content);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        TempData["Error"] = $"AI backend error: {response.StatusCode}";
        //        return RedirectToAction("Details", new { id = courseId });
        //    }

        //    var result = await response.Content.ReadAsStringAsync();
        //    TempData["Success"] = "Course sent to AI successfully! Response: " + result;

        //    return RedirectToAction("Index", new { id = courseId });
        //}

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendCourseToAI(int courseId)
        {
            var course = await unitOfWork.CourseRepository.GetCourseWithConfigsAsync(courseId);

            if (course == null || course.IsDeleted)
            {
                TempData["Error"] = "Course not found or has been deleted.";
                return RedirectToAction("Index", new { id = courseId });
            }

            if (course.CourseConfig == null || course.CourseConfig.IsDeleted)
            {
                TempData["Error"] = "Course configuration is missing or has been deleted.";
                return RedirectToAction("Index", new { id = courseId });
            }

            if (course.CourseQuestionsConfig == null || !course.CourseQuestionsConfig.Any(qc => !qc.IsDeleted))
            {
                TempData["Error"] = "Course question configurations are missing or have been deleted.";
                return RedirectToAction("Index", new { id = courseId });
            }

            // Map only non-deleted question configs
            var courseDto = new CourseGenerationDTO
            {
                CourseId = course.ID,
                Name = course.Name,
                Details = course.Details,
                BookStorageURL = course.BookStorageURL,
                ChaptersCount = course.CourseConfig.ChaptersCount,
                LessonsCountPerChapter = course.CourseConfig.LessonsCountPerChapter,
                VideoDurationInMin = course.CourseConfig.VideoDurationInMin,
                Language = course.CourseConfig.Language.ToString(),
                Persona = course.CourseConfig.Persona.ToString(),
                CourseQuestionConfig = course.CourseQuestionsConfig
                    .Where(qc => !qc.IsDeleted)
                    .Select(qc => new CourseQuestionConfigToAiDTO
                    {
                        QuestionsCountPerLesson = qc.QuestionsCountPerLesson,
                        QuestionLevelId = qc.QuestionLevelId,
                        QuestionLevelName = qc.QuestionLevel.Name
                    }).ToList()
            };

            var json = JsonSerializer.Serialize(courseDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Console.WriteLine(json );
            var response = await _httpClientFactory.CreateClient().PostAsync("https://6a42-156-214-180-25.ngrok-free.app/receive-course-config", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = $"AI backend error: {response.StatusCode}";
                return RedirectToAction("Index", new { id = courseId });
            }

            var result = await response.Content.ReadAsStringAsync();
            //  TempData["Success"] = "Course sent to AI successfully! Response: " + result;
            TempData["Success"] = "Processing started! It will take approximately 1 hour and 35 minutes.";
            return RedirectToAction("Index", new { id = courseId });
        }




    }
}
