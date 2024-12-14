
using Microsoft.AspNetCore.Mvc;
using Cumulative1.Controllers;
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

        // GET: CoursePage/New
        [HttpGet]
        public IActionResult New()
        {
            return View();
        }

        // GET: CoursePage/Validation
        [HttpGet]
        public IActionResult Validation()
        {

            if (TempData["ErrorMessage"] != null)
            {
                ViewData["ErrorMessage"] = TempData["ErrorMessage"];
            }
            return View();
        }

        // POST: CoursePage/Create
        [HttpPost]
        public IActionResult Create(Course CourseData)
        {

            // Check for future start date
            if (!string.IsNullOrEmpty(CourseData.StartDate) && DateTime.Parse(CourseData.StartDate) > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Course start date cannot be in future.";
                return RedirectToAction("Validation");
            }

            // Check for future finish date
            if (!string.IsNullOrEmpty(CourseData.FinishDate) && DateTime.Parse(CourseData.FinishDate) > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Course finish date cannot be in future.";
                return RedirectToAction("Validation");
            }

            // Check for course name field from the input
            if (string.IsNullOrEmpty(CourseData.CourseName))
            {
                TempData["ErrorMessage"] = "Course name cannot be empty";
                return RedirectToAction("Validation");
            }

            else
            {
                int CourseId = _api.AddCourse(CourseData);
                return RedirectToAction("Show", new { id = CourseId });
            }

        }


        // GET : CoursePage/DeleteConfirm/{id}
        [HttpGet]
        public IActionResult DeleteConfirm(int id)
        {
            Course SelectedCourse = _api.FindCourse(id);

            return View(SelectedCourse);
        }

        // POST: StudentPage/Delete/{id}
        [HttpPost]
        public IActionResult Delete(int id)
        {
            string RowsAffected = _api.DeleteCourse(id);

            // redirects to list action
            return RedirectToAction("List");
        }


        // GET : CoursePage/Edit/{id}
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Course SelectedCourse = _api.FindCourse(id);
            ViewData["Id"] = id;
            return View(SelectedCourse);

        }

        // POST: CoursePage/Update/{id}
        [HttpPost]
        public IActionResult Update(int id, string CourseCode, int TeacherId, string StartDate, string FinishDate, string CourseName)
        {
            Course UpdateCourse = new Course();

            UpdateCourse.CourseCode = CourseCode;
            UpdateCourse.TeacherId = TeacherId;
            UpdateCourse.StartDate = StartDate.ToString();
            UpdateCourse.FinishDate = FinishDate.ToString();
            UpdateCourse.CourseName = CourseName;

            // Check for start date
            if (string.IsNullOrEmpty(UpdateCourse.StartDate))
            {
                TempData["ErrorMessage"] = "Course start date cannot be empty.";
                return RedirectToAction("Validation");
            }

            // Check for future start date
            if (!string.IsNullOrEmpty(UpdateCourse.StartDate) && DateTime.Parse(UpdateCourse.StartDate) > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Course start date cannot be in future.";
                return RedirectToAction("Validation");
            }

            // Check for finish date
            if (string.IsNullOrEmpty(UpdateCourse.FinishDate))
            {
                TempData["ErrorMessage"] = "Course finish date cannot be empty.";
                return RedirectToAction("Validation");
            }

            // Check for future finish date
            if (!string.IsNullOrEmpty(UpdateCourse.FinishDate) && DateTime.Parse(UpdateCourse.FinishDate) > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Course finish date cannot be in future.";
                return RedirectToAction("Validation");
            }

            // Check for course name field from the input
            if (string.IsNullOrEmpty(UpdateCourse.CourseName))
            {
                TempData["ErrorMessage"] = "Course name cannot be empty";
                return RedirectToAction("Validation");
            }

            // Check for course code field from the input
            if (string.IsNullOrEmpty(UpdateCourse.CourseCode))
            {
                TempData["ErrorMessage"] = "Course code cannot be empty";
                return RedirectToAction("Validation");
            }

            // Check for teacher id field from the input
            if (UpdateCourse.TeacherId == 0)
            {
                TempData["ErrorMessage"] = "Teacher Id cannot be empty";
                return RedirectToAction("Validation");
            }

            else
            {
                _api.UpdateCourse(id, UpdateCourse);

                // redirects to show course
                return RedirectToAction("Show", new { id = id });
            }

        }

    }
}
