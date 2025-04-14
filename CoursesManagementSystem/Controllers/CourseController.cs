using AutoMapper;
using CoursesManagementSystem.Constants;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CoursesManagementSystem.Controllers
{
    public class CourseController : Controller
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly string _BookPath;
        public CourseController(IMapper _mapper, IUnitOfWork _unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            mapper = _mapper;
            unitOfWork = _unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
            _BookPath = $"{webHostEnvironment.WebRootPath}{UploadsSettings.BooksPath}";

            if (!Directory.Exists(_BookPath))
            {
                Directory.CreateDirectory(_BookPath);
            }

        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            List<CourseVM> res = await unitOfWork.CourseRepository.GetAllQuery(a => !a.IsDeleted)
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
        public async Task<IActionResult> Create()
        {
            CourseVM catLvl = new CourseVM()
            {
                Categories = await unitOfWork.CategoryRepository.GetAllAsync(c=>!c.IsDeleted),
                Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted)
            };

            return View(catLvl);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseVM courseVM)
        {
           
            if (ModelState.IsValid)
            {
                //check if course name is unique

                Course coursefound = await unitOfWork.CourseRepository.GetAsync(c => !c.IsDeleted && c.Name == courseVM.Name);
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
                    );
                if (deletedCourse != null)
                {
                    deletedCourse.IsDeleted = false;
                    await unitOfWork.CompleteAsync();
                    return RedirectToAction("Index");

                }
                if (courseVM.Book != null && courseVM.Book.Length > 0)
                {
                   
                    var BookName = $"{Path.GetFileNameWithoutExtension(courseVM.Book.FileName)}_{Guid.NewGuid()}{Path.GetExtension(courseVM.Book.FileName)}";


                    var path = Path.Combine(_BookPath, BookName);

                    // Save the file to the server
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                       await courseVM.Book.CopyToAsync(stream);
                    }

                //new Course
                     Course cmp = mapper.Map<Course>(courseVM);
                     cmp.CreatedAt = DateTime.Now;
                     cmp.BookStorageURL = $"{UploadsSettings.BooksPath}{"/"}{BookName}";// Store relative path
                     await unitOfWork.CourseRepository.AddAsync(cmp);
                     await unitOfWork.CompleteAsync();
                     return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "File upload failed.");
                    return View(courseVM);
                }
              
            }
            //refill selects

            courseVM.Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted);
            courseVM.Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted);

            return View(courseVM);

        }
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var Course = await unitOfWork.CourseRepository.GetByIdAsync(id);
            if (Course == null)
            {
                TempData["Error"] = "No Course with This Id is Found";
                return RedirectToAction("Index");
            }
            UpdateCourseVM res = mapper.Map<UpdateCourseVM>(Course);

            var Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted) ?? new List<Category>();
            var Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted) ?? new List<Level>();
            ViewBag.categorySelectList = GetCategorySelectList(Categories);
            ViewBag.levelSelectList = GetLevelSelectList(Levels);
            return View(res);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    .GetAsync(l => !l.IsDeleted && l.Name == CourseVM.Name && l.ID!=id, null, false);
                //en el id mokhtalef ma3anh enha msh ely howa byhawel ye3mlha update halyan ,laken law el esm howa howa fel submit matghyrash yb2a 3ady
                if (Course != null)
                {
                    ModelState.AddModelError("Name", "Course Name already exists ");
                    ViewBag.categorySelectList = GetCategorySelectList(Categories);
                    ViewBag.levelSelectList = GetLevelSelectList(Levels);
                    return View(CourseVM);
                }

                //check if Course is marked deleted -->mark undeleted
                var deletedCourse = await unitOfWork.CourseRepository.GetAsync(l => l.IsDeleted && l.Name == CourseVM.Name);
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

                        var BookName = $"{Path.GetFileNameWithoutExtension(CourseVM.Book.FileName)}_{Guid.NewGuid()}{Path.GetExtension(CourseVM.Book.FileName)}";


                        var path = Path.Combine(_BookPath, BookName);

                        // Save the file to the server
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await CourseVM.Book.CopyToAsync(stream);
                        }

                        // Update the file path
                        lv.BookStorageURL = $"{UploadsSettings.BooksPath}{"/"}{BookName}";
                }

                lv.LastModifiedAt = DateTime.Now;
                lv.Name = CourseVM.Name;
                lv.Details = CourseVM.Details;
                lv.CategoryId = CourseVM.CategoryId;
                lv.LevelId = CourseVM.LevelId;

                //lv.LastModifiedBy = User.Identity.Name ?? "System";
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
        public async Task<IActionResult> getById(int id)
        {
            CourseDetailsVM l = await unitOfWork.CourseRepository.GetQuery(c => !c.IsDeleted && c.ID == id)
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
        public async Task<IActionResult> Delete(int id)
        {
            Course l = await unitOfWork.CourseRepository.GetAsync(l => !l.IsDeleted && l.ID == id);
            if (l == null)
            {
                //return NotFound();

                TempData["Error"] = "No Course with this Id is Found";
                return Json(new { success = false });
            }
            //check if Course is not assigned to a courseconfig,coursequestionconfig,chapter
            var coursewithChapterfound = await unitOfWork.ChapterRepository.GetAsync(c => !c.IsDeleted && c.CourseId == id);
            var coursewithCourseConfigfound = await unitOfWork.CourseConfigRepository.GetAsync(cc => !cc.IsDeleted && cc.CourseId == id);
            var coursewithCourseQuestionConfigfound = await unitOfWork.CourseQuestionConfigRepository.GetAsync(cq => !cq.IsDeleted && cq.CourseId == id);

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

            l.IsDeleted = true;
            await unitOfWork.CompleteAsync();
            TempData["Success"] = "Course Deleted Successfully";
            return Json(new { success = true });


        }
    }
}
