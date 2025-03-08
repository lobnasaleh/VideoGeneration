using AutoMapper;
using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoursesManagementSystem.Controllers
{
    public class ChapterController : Controller
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public ChapterController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
          List<ChapterVM> res=  await unitOfWork.ChapterRepository.GetAllQuery(c=>!c.IsDeleted)
                .Select(cc=>new ChapterVM
                {
                    Details = cc.Details,
                    CourseId = cc.CourseId,
                    Name = cc.Name,
                    Sort = cc.Sort,
                    Id=cc.ID,
                    CourseName = cc.Course.Name
                })
                .ToListAsync();
            return View(res);
        }
        [HttpGet]
        public async Task<IActionResult> Create(){
        
            ViewBag.Course=await unitOfWork.CourseRepository.GetAllAsync(c=>!c.IsDeleted);
            return View();
        
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChapterVM ChapterVM)
        {
            if (ModelState.IsValid)
            {
                //check if Chapter name is unique

                Chapter Chapterfound = await unitOfWork.ChapterRepository.GetAsync(c => !c.IsDeleted && c.Name == ChapterVM.Name && c.CourseId==ChapterVM.CourseId);
                if (Chapterfound != null)
                {
                    ModelState.AddModelError("Name", "There is already a Chapter for this course with The same Name");

                    //refill selects

                    ViewBag.Course = await unitOfWork.CourseRepository.GetAllAsync(c=>!c.IsDeleted);

                    return View(ChapterVM);
                }
                //check if chapter is marked deleted -->mark undeleted
                var deletedChapter = await unitOfWork.ChapterRepository
                    .GetAsync(c => c.IsDeleted
                    && c.Details == ChapterVM.Details && c.Name == ChapterVM.Name
                    && c.CourseId == ChapterVM.CourseId && c.Sort == ChapterVM.Sort
                    );
                if (deletedChapter != null)
                {
                    deletedChapter.IsDeleted = false;
                    await unitOfWork.CompleteAsync();
                    return RedirectToAction("Index");

                }
                //new Chapter
                Chapter cmp = mapper.Map<Chapter>(ChapterVM);
                cmp.CreatedAt = DateTime.Now;
                //cmp.CreatedBy = User.Identity.Name ?? "System";
                await unitOfWork.ChapterRepository.AddAsync(cmp);
                await unitOfWork.CompleteAsync();
                return RedirectToAction("Index");

            }
            //refill selects

            ViewBag.Course = await unitOfWork.CourseRepository.GetAllAsync(c=>!c.IsDeleted);

            return View(ChapterVM);

        }


        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var Chapter = await unitOfWork.ChapterRepository.GetByIdAsync(id);
            if (Chapter == null)
            {
                TempData["Error"] = "No Chapter with This Id is Found";
                return RedirectToAction("Index");
            }
            ChapterVM res = mapper.Map<ChapterVM>(Chapter);

            ViewBag.Course = await unitOfWork.CourseRepository.GetAllAsync(c=>!c.IsDeleted);
            return View(res);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, ChapterVM ChapterVM)
        {
            if (ModelState.IsValid)
            {

                Chapter lv = await unitOfWork.ChapterRepository.GetByIdAsync(id);
                if (lv == null)
                {
                    TempData["Error"] = "No Chapter with This Id is Found";
                    return RedirectToAction("Index");
                }
                //check if it is unique
                var chapter= await unitOfWork.ChapterRepository
                    .GetAsync(l => !l.IsDeleted && l.Name == ChapterVM.Name && l.CourseId== ChapterVM.CourseId && l.ID != id, null, false);
                //en el id mokhtalef ma3anh enha msh ely howa byhawel ye3mlha update halyan ,laken law el esm howa howa fel submit matghyrash yb2a 3ady
                if (chapter != null)
                {
                    ModelState.AddModelError("Name", "There is already a Chapter for this course with The same Name ");
                    ViewBag.Course = await unitOfWork.CourseRepository.GetAllAsync(c=>!c.IsDeleted);
                    return View(ChapterVM);
                }

                //check if Chapter is marked deleted -->mark undeleted
                var deletedChapter = await unitOfWork.ChapterRepository.GetAsync(l => l.IsDeleted &&
                l.Name == ChapterVM.Name && l.CourseId==ChapterVM.CourseId
                && l.Details==ChapterVM.Details && l.Sort == ChapterVM.Sort);
                if (deletedChapter != null)
                {
                    lv.IsDeleted = true;
                    deletedChapter.IsDeleted = false;
                    await unitOfWork.CompleteAsync();
                    return RedirectToAction("Index");

                }
                lv.LastModifiedAt = DateTime.Now;
                lv.Name = ChapterVM.Name;
                lv.Details = ChapterVM.Details;
                lv.CourseId = ChapterVM.CourseId;
                lv.Sort = ChapterVM.Sort;
              

                //lv.LastModifiedBy = User.Identity.Name ?? "System";
                await unitOfWork.CompleteAsync();
                return RedirectToAction("Index");

            }
            ViewBag.Course = await unitOfWork.CourseRepository.GetAllAsync(c => !c.IsDeleted);

            return View(ChapterVM);

        }
        [HttpGet]
        public async Task<IActionResult> getById(int id)
        {
          
                ChapterDetailsVM cv=await unitOfWork.ChapterRepository.GetQuery(c=>!c.IsDeleted && c.ID==id)
                .Select(cc=>new ChapterDetailsVM
                {
                    CourseId = cc.CourseId,
                    Name = cc.Name,
                    CourseName=cc.Course.Name,
                    Details = cc.Details,
                    Id = id,
                    Sort=cc.Sort,
                    LastModifiedBy = cc.LastModifiedBy,
                    CreatedBy=cc.CreatedBy,
                    CreatedAt=cc.CreatedAt,
                    LastModifiedAt=cc.LastModifiedAt
                    
                })
                .FirstOrDefaultAsync();

            if (cv == null)
            {
                // return NotFound();

                TempData["Error"] = "No Chapter with This Id is Found";
                return RedirectToAction("Index");
            }
            return View(cv);
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            Chapter l = await unitOfWork.ChapterRepository.GetAsync(l => !l.IsDeleted && l.ID == id);
            if (l == null)
            {
                //return NotFound();

                TempData["Error"] = "No Chapter with this Id is Found";
                return RedirectToAction("Index");
            }
            return View(l);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            Chapter l = await unitOfWork.ChapterRepository.GetAsync(l => !l.IsDeleted && l.ID == id);
            if (l == null)
            {
                //return NotFound();

                TempData["Error"] = "No Chapter with this Id is Found";
                return RedirectToAction("Index");
            }
            //check if Chapter is not assigned to a courseconfig,coursequestionconfig,chapter
            var LessonWithChapterfound = await unitOfWork.LessonRepository.GetAsync(l => !l.IsDeleted && l.ChapterId == id);
          
            if (LessonWithChapterfound != null)
            {
                //return BadRequest();
                TempData["Error"] = "Can not delete a Chapter having Lessons";
                return RedirectToAction("Index");
            }
            
            l.IsDeleted = true;
            await unitOfWork.CompleteAsync();
            return RedirectToAction("Index");


        }









    }
}
