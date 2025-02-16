using Microsoft.AspNetCore.Mvc;

namespace CoursesManagementSystem.Controllers
{
    public class CourseController : Controller
    {
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ConfirmAdd()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Track()
        {
            return View();
        }

        [HttpGet]
        public IActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Generate()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Details()
        {
            return View();
        }

    }
}
