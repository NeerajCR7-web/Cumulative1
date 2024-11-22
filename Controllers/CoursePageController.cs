using Cumulative1.Controllers;
using Microsoft.AspNetCore.Mvc;
using Cumulative1.Models;

namespace Cumulative1.Controllers
{
    public class CoursePageController : Controller
    {
        // currently relying on the API to retrieve author information
        // this is a simplified example. In practice, both the CourseAPI and CoursePage controllers
        // should rely on a unified "Service", with an explicit interface
        private readonly CourseAPIController _api;

        public CoursePageController(CourseAPIController api)
        {
            _api = api;
        }

        // GET : CoursePage/List
        public IActionResult List()
        {
            List<Course> Courses = _api.ListCourses();
            return View(Courses);
        }

        // GET : CoursePage/Show/{id}
        public IActionResult Show(int id)
        {
            Course SelectedCourse = _api.FindCourse(id);
            ViewData["Id"] = id;
            return View(SelectedCourse);
        }
    }
}

