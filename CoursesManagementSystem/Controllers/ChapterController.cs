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
        
            ViewBag.Course=await unitOfWork.CourseRepository.GetAllAsync();
            return View();
        
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChapterVM ChapterVM)
        {
            if (ModelState.IsValid)
            {
                //check if course name is unique

                Chapter Chapterfound = await unitOfWork.ChapterRepository.GetAsync(c => !c.IsDeleted && c.Name == ChapterVM.Name && c.CourseId==ChapterVM.CourseId);
                if (Chapterfound != null)
                {
                    ModelState.AddModelError("Name", "There is already a Chapter for this course with The same Name");

                    //refill selects

                    ViewBag.Course = await unitOfWork.CourseRepository.GetAllAsync();

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

            ViewBag.Course = await unitOfWork.CourseRepository.GetAllAsync();

            return View(ChapterVM);

        }



    }
}
