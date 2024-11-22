using Cumulative1.Controllers;
using Microsoft.AspNetCore.Mvc;
using Cumulative1.Models;

namespace Cumulative1.Controllers
{
    public class TeacherPageController : Controller
    {
        // currently relying on the API to retrieve author information
        // this is a simplified example. In practice, both the TeacherAPI and TeacherPage controllers
        // should rely on a unified "Service", with an explicit interface
        private readonly TeacherAPIController _api;

        public TeacherPageController(TeacherAPIController api)
        {
            _api = api;
        }

        // GET : TeacherPage/List
        [HttpGet]
        public IActionResult List()
        {
            var model = new TeacherSearchModel
            {
                // Get all teachers initially
                Teachers = _api.ListTeachers()
            };

            return View(model);
        }

        // POST : TeacherPage/List
        // POST Data : TeacherSearchModel model
        [HttpPost]
        public IActionResult List(TeacherSearchModel model)
        {
            // Get all teachers from the API
            List<Teacher> Teachers = _api.ListTeachers();

            // Filter teachers by HireDate if StartDate and EndDate are provided
            if (!string.IsNullOrEmpty(model.StartDate) && !string.IsNullOrEmpty(model.EndDate))
            {
                DateTime start = DateTime.Parse(model.StartDate);
                DateTime end = DateTime.Parse(model.EndDate);

                Teachers = Teachers.Where(teacher => DateTime.Parse(teacher.HireDate) >= start && DateTime.Parse(teacher.HireDate) <= end).ToList();
            }

            // Set the filtered list of teachers and return the model to the view
            model.Teachers = Teachers;

            // Pass the model to the view
            return View(model);
        }

        // GET : TeacherPage/Show/{id}
        public IActionResult Show(int id)
        {
            Teacher SelectedTeacher = _api.FindTeacher(id);
            ViewData["Id"] = id;
            return View(SelectedTeacher);


        }

    }
}

