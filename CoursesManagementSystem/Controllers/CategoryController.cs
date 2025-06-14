﻿using CoursesManagementSystem.DB.Models;
using CoursesManagementSystem.Interfaces;
using CoursesManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var categories = await unitOfWork.CategoryRepository.GetAllAsync(c=>!c.IsDeleted);
           

            return View(categories);
        }
        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Category category)
        {
            if(ModelState.IsValid)
            {
                category.CreatedAt = DateTime.UtcNow;
                category.CreatedBy = User.Identity.Name ?? "System";

                //check if category with this name already exists
                var cat = await unitOfWork.CategoryRepository.GetAsync(c => !c.IsDeleted && c.Name == category.Name);
                if (cat is not null)
                {
                    ModelState.AddModelError("Name", "A Category With This Name already exists");
                    return View(category);

                }

                //check if category is already marked deleted instead of inserting another row -->mark undeleted

                var foundcategory = await unitOfWork.CategoryRepository.GetAsync(c=>c.IsDeleted);
                if (foundcategory is not null)
                {
                    foundcategory.IsDeleted=false;
                    foundcategory.LastModifiedAt = DateTime.UtcNow;
                    //foundcategory.LastModifiedBy = User.Identity.Name ?? "System";

                    await unitOfWork.CompleteAsync();
                    return RedirectToAction(nameof(GetAll));
                }
                await unitOfWork.CategoryRepository.AddAsync(category);
                await unitOfWork.CompleteAsync();
                return RedirectToAction(nameof(GetAll));
            }

            return View(category);
        }

        [HttpGet]
        [Authorize]
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
        [Authorize]
        public async Task<IActionResult> Edit(int id , Category category)
        {
            if(id != category.ID)
                return BadRequest();


            if (ModelState.IsValid)
            {
                var existcategory = await unitOfWork.CategoryRepository.GetByIdAsync(id);

                if(existcategory == null)
                    return BadRequest();

                //if category is found but deleted ,mark undeleted and delete the existing one
                var foundcategory = await unitOfWork.CategoryRepository.GetAsync(c => c.IsDeleted && c.Name == category.Name );
                if (foundcategory is not null)
                {
                    existcategory.IsDeleted=true;
                    foundcategory.IsDeleted = false;
                    await unitOfWork.CompleteAsync();
                    return RedirectToAction(nameof(GetAll));
                }
                //Name?
                existcategory.LastModifiedAt = DateTime.UtcNow;
                existcategory.LastModifiedBy = User.Identity.Name ?? "System";
                existcategory.Name = category.Name;
              
                unitOfWork.CategoryRepository.Update(existcategory);
                await unitOfWork.CompleteAsync();

                return RedirectToAction(nameof(GetAll));

                
            }

            return View(category);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> getById(int id)
    {
        //hall fe el get hahtag a3mel view model lel category law ana keda keda ana bakhtar ha3red eh?
        Category c = await unitOfWork.CategoryRepository.GetAsync(c => !c.IsDeleted && c.ID == id );
        if (c == null)
        {
                // return NotFound();

                TempData["Error"] = "No Course Category with This Id is Found";
                return RedirectToAction("GetAll");
        }

        return View(c);
    }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> getByName(string Name)
    {
        Category c = await unitOfWork.CategoryRepository.GetAsync(c => !c.IsDeleted && c.Name == Name );

        if (c == null)
        {
            return NotFound();
            //or
            //TempData["Error"]="No Course Category with Name is Found"
            //return RedirecToAction("GetAll")
        }
        return View(c);
    }
   
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
    {
        Category c = await unitOfWork.CategoryRepository.GetAsync(cat => !cat.IsDeleted && cat.ID == id );

        if (c == null)
        {
                // return NotFound();

                TempData["Error"] = "No Course Category with Id is Found";
                return Json(new { success = false });
            }
        //check if Category is not assigned to a course
        var coursewithcategoryfound = await unitOfWork.CourseRepository.GetAsync(c => !c.IsDeleted && c.CategoryId == id && c.CreatedBy == User.Identity.Name);

        if (coursewithcategoryfound != null)
        {
                //return BadRequest();

                TempData["Error"] = "Can not delete a Category that is assigned to a Course";
                return Json(new { success = false });
            }

            c.IsDeleted = true;
            await unitOfWork.CompleteAsync();
            TempData["Success"] = "Category Deleted Successfully";
            return Json(new { success = true });

        }



    }
}
