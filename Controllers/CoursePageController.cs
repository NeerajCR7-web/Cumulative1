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

        /// <summary>
        /// Retrieves and displays a list of all courses from the API.
        /// </summary>
        /// <returns>A view displaying a list of courses.</returns>
        public IActionResult List()
        {
            List<Course> Courses = _api.ListCourses();
            return View(Courses);
        }

        /// <summary>
        /// Retrieves and displays details of a specific course based on the given ID.
        /// </summary>
        /// <param name="id">The ID of the course to display.</param>
        /// <returns>A view displaying the details of the selected course.</returns>
        public IActionResult Show(int id)
        {
            Course SelectedCourse = _api.FindCourse(id);
            ViewData["Id"] = id;
            return View(SelectedCourse);
        }

        /// <summary>
        /// Displays a form for creating a new course.
        /// </summary>
        /// <returns>A view containing the course creation form.</returns>
        [HttpGet]
        public IActionResult New()
        {
            return View();
        }

        /// <summary>
        /// Displays validation errors, if any, for the course creation process.
        /// </summary>
        /// <returns>A view showing validation error messages.</returns>
        [HttpGet]
        public IActionResult Validation()
        {
            if (TempData["ErrorMessage"] != null)
            {
                ViewData["ErrorMessage"] = TempData["ErrorMessage"];
            }
            return View();
        }

        /// <summary>
        /// Validates and processes a new course creation request.
        /// </summary>
        /// <param name="CourseData">The course data submitted by the user.</param>
        /// <returns>
        /// Redirects to the validation page if there are errors, or the course details page if creation is successful.
        /// </returns>
        [HttpPost]
        public IActionResult Create(Course CourseData)
        {
            if (!string.IsNullOrEmpty(CourseData.StartDate) && DateTime.Parse(CourseData.StartDate) > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Course start date cannot be in future.";
                return RedirectToAction("Validation");
            }

            if (!string.IsNullOrEmpty(CourseData.FinishDate) && DateTime.Parse(CourseData.FinishDate) > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Course finish date cannot be in future.";
                return RedirectToAction("Validation");
            }

            if (string.IsNullOrEmpty(CourseData.CourseName))
            {
                TempData["ErrorMessage"] = "Course name cannot be empty.";
                return RedirectToAction("Validation");
            }

            int CourseId = _api.AddCourse(CourseData);
            return RedirectToAction("Show", new { id = CourseId });
        }

        /// <summary>
        /// Displays a confirmation page before deleting a course.
        /// </summary>
        /// <param name="id">The ID of the course to delete.</param>
        /// <returns>A view asking for confirmation to delete the selected course.</returns>
        [HttpGet]
        public IActionResult DeleteConfirm(int id)
        {
            Course SelectedCourse = _api.FindCourse(id);
            return View(SelectedCourse);
        }

        /// <summary>
        /// Deletes a course based on the provided ID.
        /// </summary>
        /// <param name="id">The ID of the course to delete.</param>
        /// <returns>
        /// Redirects to the course list view after the delete operation.
        /// </returns>
        [HttpPost]
        public IActionResult Delete(int id)
        {
            string RowsAffected = _api.DeleteCourse(id);
            return RedirectToAction("List");
        }
    }
}
