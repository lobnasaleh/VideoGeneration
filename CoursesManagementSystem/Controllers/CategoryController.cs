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

            return View(c);
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
                //or
                //TempData["Error"]="No Course Category with Id is Found"
                //return RedirecToAction("GetAll")
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


    [HttpGet]
    public async Task<IActionResult> getById(int id)
    {
        //hall fe el get hahtag a3mel view model lel category law ana keda keda ana bakhtar ha3red eh?
        Category c = await unitOfWork.CategoryRepository.GetAsync(c => !c.IsDeleted && c.ID == id);
        if (c == null)
        {
            return NotFound();
            //or
            //TempData["Error"]="No Course Category with Id is Found"
            //return RedirecToAction("GetAll")
        }

        return View(c);
    }
    [HttpGet]
    public async Task<IActionResult> getByName(string Name)
    {
        Category c = await unitOfWork.CategoryRepository.GetAsync(c => !c.IsDeleted && c.Name == Name);

        if (c == null)
        {
            return NotFound();
            //or
            //TempData["Error"]="No Course Category with Name is Found"
            //return RedirecToAction("GetAll")
        }
        return View(c);
    }
    [HttpGet]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        Category c = await unitOfWork.CategoryRepository.GetAsync(c => !c.IsDeleted && c.ID == id);
        if (c == null)
        {
            return NotFound();
            //or
            //TempData["Error"]="No Course Category with Id is Found"
            //return RedirecToAction("GetAll")
        }
        return View(c);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        Category c = await unitOfWork.CategoryRepository.GetAsync(cat => !cat.IsDeleted && cat.ID == id);

        if (c == null)
        {
            return NotFound();
            //or
            //TempData["Error"]="No Course Category with Id is Found"
            //return RedirecToAction("GetAll")
        }
        //check if Category is not assigned to a course
        var coursewithcategoryfound = await unitOfWork.CourseRepository.GetAsync(c => !c.IsDeleted && c.CategoryId == id);

        if (coursewithcategoryfound != null)
        {
            return BadRequest();
            //or
            //Viewbag.Error="Can not delete a Category that is not assigned to a Course";
            //return View(c);
        }

        c.IsDeleted = true;
        await unitOfWork.CompleteAsync();
        return View(c);


    }



}
}
