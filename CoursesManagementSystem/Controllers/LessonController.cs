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
    public class LessonController : Controller
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly string _VideosPath;
        private readonly string _AudiosPath;



        public LessonController(IMapper mapper, IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
            _VideosPath = $"{webHostEnvironment.WebRootPath}{UploadsSettings.VideosPath}";

            if (!Directory.Exists(_VideosPath))
            {
                Directory.CreateDirectory(_VideosPath);
            }

            _AudiosPath = $"{webHostEnvironment.WebRootPath}{UploadsSettings.AudiosPath}";

            if (!Directory.Exists(_AudiosPath))
            {
                Directory.CreateDirectory(_AudiosPath);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<LessonVM> lessons = await unitOfWork.LessonRepository.GetAllQuery(l=>!l.IsDeleted)
                .Select(l => new LessonVM
                {
                   // AudioStorageURL = l.AudioStorageURL,
                    ChapterId = l.ChapterId,
                    ChapterName = l.Chapter.Name,
                    CourseName=l.Chapter.Course.Name,
                    Name = l.Name,
                    Details = l.Details,
                    ScriptText = l.ScriptText,
                    Sort = l.Sort,
                  //  VideoStorageURL = l.VideoStorageURL,
                    Id=l.ID
                })
                .ToListAsync();
            return View(lessons);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Chapters = await unitOfWork.ChapterRepository.GetAllQuery(c => !c.IsDeleted)
                 .Select(l => new SelectListItem
                 {
                     Value = l.ID.ToString(),
                     Text = $"{l.Name} : {l.Course.Name}"
                 })
                 .ToListAsync();

            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LessonVM LessonVM)
        {
            string AudioName, VideoName;
            if (ModelState.IsValid)
            {
                //check if Lesson name is unique

               Lesson Lessonfound = await unitOfWork.LessonRepository.GetAsync(c => !c.IsDeleted && c.Name == LessonVM.Name && c.ChapterId==LessonVM.ChapterId);
                if (Lessonfound != null)
                {
                    ModelState.AddModelError("Name", "There is already a Lesson for this Chapter with The same Name");

                    //refill selects
                    ViewBag.Chapters = await GetChaptersSelectList();
                    return View(LessonVM);
                }
                //check if Lesson is marked deleted -->mark undeleted
                var deletedLesson = await unitOfWork.LessonRepository
                    .GetAsync(c => c.IsDeleted && c.Sort == LessonVM.Sort
                    && c.Details == LessonVM.Details && c.Name == LessonVM.Name
                    && c.ChapterId == LessonVM.ChapterId 
                   // && c.AudioStorageURL == LessonVM.AudioStorageURL
                    //&& c.VideoStorageURL == LessonVM.VideoStorageURL
                  //  && c.ScriptText == LessonVM.ScriptText
                    );
                if (deletedLesson != null)
                {
                    deletedLesson.IsDeleted = false;
                    deletedLesson.LastModifiedAt = DateTime.Now;
                    //deletedLesson.LastModifiedBy=
                    await unitOfWork.CompleteAsync();
                    return RedirectToAction("Index");

                }
                //Video Upload
                if (LessonVM.Video != null && LessonVM.Video.Length > 0)
                {
                    VideoName = $"{Path.GetFileNameWithoutExtension(LessonVM.Video.FileName)}_{Guid.NewGuid()}{Path.GetExtension(LessonVM.Video.FileName)}";

                    var path = Path.Combine(_VideosPath, VideoName);

                    // Save the file to the server
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await LessonVM.Video.CopyToAsync(stream);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Video upload failed.");
                    ViewBag.Chapters = await GetChaptersSelectList();
                    return View(LessonVM);
                }

                //Audio Upload
                if (LessonVM.Audio != null && LessonVM.Audio.Length > 0)
                {

                     AudioName = $"{Path.GetFileNameWithoutExtension(LessonVM.Audio.FileName)}_{Guid.NewGuid()}{Path.GetExtension(LessonVM.Audio.FileName)}";

                    var path = Path.Combine(_AudiosPath, AudioName);

                    // Save the file to the server
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await LessonVM.Audio.CopyToAsync(stream);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Audio upload failed.");
                    ViewBag.Chapters = await GetChaptersSelectList();
                    return View(LessonVM);//refill selects?
                }

                //new Lesson
                Lesson cmp = mapper.Map<Lesson>(LessonVM);
                cmp.CreatedAt = DateTime.Now;
                cmp.VideoStorageURL = $"{UploadsSettings.VideosPath}{"/"}{VideoName}";
                cmp.AudioStorageURL = $"{UploadsSettings.AudiosPath}{"/"}{AudioName}";

                //cmp.CreatedBy = User.Identity.Name ?? "System";
                await unitOfWork.LessonRepository.AddAsync(cmp);
                await unitOfWork.CompleteAsync();
                return RedirectToAction("Index");

            }
            //refill selects
            ViewBag.Chapters =await GetChaptersSelectList();
            return View(LessonVM);

        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var Lesson= await unitOfWork.LessonRepository.GetByIdAsync(id);
            if (Lesson == null)
            {
                TempData["Error"] = "No Lesson with This Id is Found";
                return RedirectToAction("Index");
            }
            UpdateLessonVM res = mapper.Map<UpdateLessonVM>(Lesson);

            ViewBag.Chapters = await unitOfWork.ChapterRepository.GetAllQuery(c => !c.IsDeleted)
                  .Select(l => new SelectListItem
                  {
                      Value = l.ID.ToString(),
                      Text = $"{l.Name} : {l.Course.Name}"
                  })
                  .ToListAsync();
            return View(res);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, UpdateLessonVM LessonVM)
        {
            if (ModelState.IsValid)
            {

                Lesson lv = await unitOfWork.LessonRepository.GetByIdAsync(id);
                if (lv == null)
                {
                    TempData["Error"] = "No Lesson with This Id is Found";
                    return RedirectToAction("Index");
                }
                //check if it is unique
                var Lesson = await unitOfWork.LessonRepository
                    .GetAsync(l => !l.IsDeleted && l.Name == LessonVM.Name && l.ChapterId == LessonVM.ChapterId && l.ID != id, null, false);
                //en el id mokhtalef ma3anh enha msh ely howa byhawel ye3mlha update halyan ,laken law el esm howa howa fel submit matghyrash yb2a 3ady
                if (Lesson != null)
                {
                    ModelState.AddModelError("Name", "There is already a Lesson for this Chapter with The same Name ");
                    ViewBag.Chapters = await unitOfWork.ChapterRepository.GetAllAsync(c => !c.IsDeleted);
                    return View(LessonVM);
                }

                //check if Lesson is marked deleted -->mark undeleted
                var deletedLesson = await unitOfWork.LessonRepository
                      .GetAsync(c => c.IsDeleted && c.Sort == LessonVM.Sort
                      && c.Details == LessonVM.Details && c.Name == LessonVM.Name
                      && c.ChapterId == LessonVM.ChapterId
                    //  && c.AudioStorageURL == LessonVM.AudioStorageURL
                     // && c.VideoStorageURL == LessonVM.VideoStorageURL
                      && c.ScriptText == LessonVM.ScriptText
                      );
                if (deletedLesson != null)
                {
                    lv.IsDeleted = true;
                    deletedLesson.IsDeleted = false;
                    await unitOfWork.CompleteAsync();
                    return RedirectToAction("Index");

                }
                //Video Edit
                if (LessonVM.Video != null && LessonVM.Video.Length > 0)
                {

                    // Delete the old file if it exists
                    if (!string.IsNullOrEmpty(lv.VideoStorageURL))
                    {
                        var oldFilePath = Path.Combine(webHostEnvironment.WebRootPath, lv.VideoStorageURL.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var VideoName = $"{Path.GetFileNameWithoutExtension(LessonVM.Video.FileName)}_{Guid.NewGuid()}{Path.GetExtension(LessonVM.Video.FileName)}";
                    var path = Path.Combine(_VideosPath, VideoName);

                    // Save the file to the server
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await LessonVM.Video.CopyToAsync(stream);
                    }

                    // Update the file path
                    lv.VideoStorageURL = $"{UploadsSettings.VideosPath}{"/"}{VideoName}";
                }

                //Audio Edit
                if (LessonVM.Audio != null && LessonVM.Audio.Length > 0)
                {

                    // Delete the old file if it exists
                    if (!string.IsNullOrEmpty(lv.AudioStorageURL))
                    {
                        var oldFilePath = Path.Combine(webHostEnvironment.WebRootPath, lv.AudioStorageURL.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var AudioName = $"{Path.GetFileNameWithoutExtension(LessonVM.Audio.FileName)}_{Guid.NewGuid()}{Path.GetExtension(LessonVM.Audio.FileName)}";
                    var path = Path.Combine(_AudiosPath, AudioName);

                    // Save the file to the server
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await LessonVM.Audio.CopyToAsync(stream);
                    }

                    // Update the file path
                    lv.AudioStorageURL = $"{UploadsSettings.AudiosPath}{"/"}{AudioName}";
                }
                lv.LastModifiedAt = DateTime.Now;
                //lv.LastModifiedBy =;
                lv.Name = LessonVM.Name;
                lv.Details = LessonVM.Details;
                lv.ChapterId = LessonVM.ChapterId;
                lv.Sort = LessonVM.Sort;
                lv.ScriptText = LessonVM.ScriptText;
                lv.Sort=LessonVM.Sort;
                

                //lv.LastModifiedBy = User.Identity.Name ?? "System";
                await unitOfWork.CompleteAsync();
                return RedirectToAction("Index");

            }
            ViewBag.Chapters = await unitOfWork.ChapterRepository.GetAllQuery(c => !c.IsDeleted)
                 .Select(l => new SelectListItem
                 {
                     Value = l.ID.ToString(),
                     Text = $"{l.Name} : {l.Course.Name}"
                 })
                 .ToListAsync();

            return View(LessonVM);

        }

        [HttpGet]
        public async Task<IActionResult> getById(int id)
        {

            LessonDetailsVM cv = await unitOfWork.LessonRepository.GetQuery(c => !c.IsDeleted && c.ID == id)
            .Select(l => new LessonDetailsVM
            {
                AudioStorageURL = l.AudioStorageURL,
                ChapterId = l.ChapterId,
                ChapterName = l.Chapter.Name,
                CourseName=l.Chapter.Course.Name,
                Name = l.Name,
                Details = l.Details,
                ScriptText = l.ScriptText,
                Sort = l.Sort,
                VideoStorageURL = l.VideoStorageURL,
                Id = l.ID,
                LastModifiedAt = l.LastModifiedAt,
                CreatedAt = l.CreatedAt,
                CreatedBy = l.CreatedBy,
                LastModifiedBy = l.LastModifiedBy

            })
            .FirstOrDefaultAsync();

            if (cv == null)
            {
                // return NotFound();

                TempData["Error"] = "No Lesson with This Id is Found";
                return RedirectToAction("Index");
            }
            return View(cv);
        }


        private async Task<List<SelectListItem>> GetChaptersSelectList()
        {
           return await unitOfWork.ChapterRepository.GetAllQuery(c => !c.IsDeleted)
                  .Select(l => new SelectListItem
                  {
                      Value = l.ID.ToString(),
                      Text = $"{l.Name} : {l.Course.Name}"
                  })
                  .ToListAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            Lesson l = await unitOfWork.LessonRepository.GetAsync(l => !l.IsDeleted && l.ID == id);
            if (l == null)
            {
                //return NotFound();

                TempData["Error"] = "No Lesson with this Id is Found";
                return Json(new { success = false});
            }
            //check if Lesson is not assigned to a Question
            var QuestionWithLessonfound = await unitOfWork.QuestionRepository.GetAsync(q => !q.IsDeleted && q.LessonId == id);

            if (QuestionWithLessonfound != null)
            {
                //return BadRequest();
                TempData["Error"] = "Can not delete a Lesson having Questions";
                return Json(new { success = false });
            }
            // Delete the file if it exists
            if (!string.IsNullOrEmpty(l.VideoStorageURL))
            {
                var oldFilePath = Path.Combine(webHostEnvironment.WebRootPath, l.VideoStorageURL.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }
            // Delete the file if it exists
            if (!string.IsNullOrEmpty(l.AudioStorageURL))
            {
                var oldFilePath = Path.Combine(webHostEnvironment.WebRootPath, l.AudioStorageURL.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }


            l.IsDeleted = true;
            await unitOfWork.CompleteAsync();
            TempData["Success"] = "Lesson Deleted Successfully";
            return Json(new { success = true });

        }


    }
}
