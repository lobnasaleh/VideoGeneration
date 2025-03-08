using AutoMapper;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoursesManagementSystem.Controllers
{
    public class CourseController : Controller
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public CourseController(IMapper _mapper, IUnitOfWork _unitOfWork)
        {
            mapper = _mapper;
            unitOfWork = _unitOfWork;
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
                    BookStorageURL = c.BookStorageURL,
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
                //new Course
                Course cmp = mapper.Map<Course>(courseVM);
                cmp.CreatedAt = DateTime.Now;
                //cmp.CreatedBy = User.Identity.Name ?? "System";
                await unitOfWork.CourseRepository.AddAsync(cmp);
                await unitOfWork.CompleteAsync();
                return RedirectToAction("Index");

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
            CourseVM res = mapper.Map<CourseVM>(Course);

            res.Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted);
                res.Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted);
            return View(res);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, CourseVM CourseVM)
        {
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
                    CourseVM.Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted);
                    CourseVM.Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted);

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
                lv.LastModifiedAt = DateTime.Now;
                lv.Name = CourseVM.Name;
                lv.Details = CourseVM.Details;
                lv.CategoryId = CourseVM.CategoryId;
                lv.LevelId = CourseVM.LevelId;
                lv.BookStorageURL = CourseVM.BookStorageURL;




                //lv.LastModifiedBy = User.Identity.Name ?? "System";
                await unitOfWork.CompleteAsync();
                return RedirectToAction("Index");

            }
            CourseVM.Categories = await unitOfWork.CategoryRepository.GetAllAsync(c => !c.IsDeleted);
            CourseVM.Levels = await unitOfWork.LevelRepository.GetAllAsync(c => !c.IsDeleted);

            return View(CourseVM);

        }
        [HttpGet]
        public async Task<IActionResult> getById(int id)
        {
            CourseVM l = await unitOfWork.CourseRepository.GetQuery(c => !c.IsDeleted && c.ID == id)
                .Select(c => new CourseVM
                {
                    CategoryName = c.Category.Name,
                    LevelName = c.Level.Name,
                    Name = c.Name,
                    Details = c.Details,
                    BookStorageURL = c.BookStorageURL,
                    CategoryId = c.CategoryId,
                    LevelId = c.LevelId,
                    Id = id

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
        [HttpGet]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            Course l = await unitOfWork.CourseRepository.GetAsync(l => !l.IsDeleted && l.ID == id);
            if (l == null)
            {
                //return NotFound();

                TempData["Error"] = "No Course with this Id is Found";
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
                return RedirectToAction("Index");
            }
            //check if Course is not assigned to a courseconfig,coursequestionconfig,chapter
            var coursewithChapterfound = await unitOfWork.ChapterRepository.GetAsync(c => !c.IsDeleted && c.CourseId == id);
            var coursewithCourseConfigfound = await unitOfWork.CourseConfigRepository.GetAsync(cc => !cc.IsDeleted && cc.CourseId == id);
            var coursewithCourseQuestionConfigfound = await unitOfWork.CourseQuestionConfigRepository.GetAsync(cq => !cq.IsDeleted && cq.CourseId == id);

            if (coursewithChapterfound != null)
            {
                //return BadRequest();
                TempData["Error"] = "Can not delete a Course that is assigned to a Chapter";
                return RedirectToAction("Index");
            }
            if (coursewithCourseConfigfound != null || coursewithCourseQuestionConfigfound != null)
            {
                //return BadRequest();
                TempData["Error"] = "Can not delete a Course that has Configurations";
                return RedirectToAction("Index");
            }
            l.IsDeleted = true;
            await unitOfWork.CompleteAsync();
            return RedirectToAction("Index");


        }
    }
}
