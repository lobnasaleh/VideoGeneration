using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.Repository;
using Microsoft.AspNetCore.Mvc;

namespace CoursesManagementSystem.Controllers
{
    public class CategoryController : Controller
    {
        //private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;

        }
        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> GetAll()
        {
            var categories = await unitOfWork.CategoryRepository.GetAllAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if(ModelState.IsValid)
            {
                category.CreatedAt = DateTime.UtcNow;
               // category.CreatedBy = User.Identity.Name ?? "System";
                await unitOfWork.CategoryRepository.AddAsync(category);
                //  _categoryRepository.AddAsync(category);
                await unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(GetAll));
            }

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await unitOfWork.CategoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id , Category category)
        {
            if(id != category.ID)
                return BadRequest();


            if (ModelState.IsValid)
            {
                var existcategory = await unitOfWork.CategoryRepository.GetByIdAsync(id);

                if(existcategory == null)
                    return BadRequest();

                existcategory.LastModifiedAt = DateTime.UtcNow;
                //existcategory.LastModifiedBy = User.Identity.Name ?? "System";
                existcategory.LastModifiedBy = category.LastModifiedBy;

                unitOfWork.CategoryRepository.Update(existcategory);
                await unitOfWork.CompleteAsync();

                return RedirectToAction(nameof(GetAll));

                
            }

            return View(category);
        }




    }
}
