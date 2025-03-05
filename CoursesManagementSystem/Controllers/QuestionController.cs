using Microsoft.AspNetCore.Mvc;

namespace CoursesManagementSystem.Controllers
{
    public class QuestionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
