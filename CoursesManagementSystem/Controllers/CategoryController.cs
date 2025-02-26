using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoursesManagementSystem.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> getById(int id)
        {
            //hall fe el get hahtag a3mel view model lel category law ana keda keda ana bakhtar ha3red eh?
            Category c =  await unitOfWork.CategoryRepository.GetAsync(c=> !c.IsDeleted && c.ID==id);
            if (c == null) { 
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
            Category c = await unitOfWork.CategoryRepository.GetAsync(c => !c.IsDeleted && c.Name==Name);

            if (c == null) {
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
            Category c = await unitOfWork.CategoryRepository.GetAsync(c => !c.IsDeleted && c.ID==id);
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
            var coursewithcategoryfound= await unitOfWork.CourseRepository.GetAsync(c=> !c.IsDeleted &&c.CategoryId==id);

            if (coursewithcategoryfound != null) {
                return BadRequest();
                //or
                //Viewbag.Error="Can not delete a Category that is not assigned to a Course";
                //return View(c);
            }

            c.IsDeleted=true;
           await unitOfWork.CompleteAsync();
            return View(c);


        }






    }
}
