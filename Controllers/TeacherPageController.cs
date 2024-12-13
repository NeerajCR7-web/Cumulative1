using Cumulative1.Controllers;
using Cumulative1.Models;
using Microsoft.AspNetCore.Mvc;
using Cumulative1.Models;
using System.Text.RegularExpressions;

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

        // GET: TeacherPage/New
        [HttpGet]
        public IActionResult New(int id)
        {
            return View();
        }

        // GET: TeacherPage/Validation
        [HttpGet]
        public IActionResult Validation()
        {

            if (TempData["ErrorMessage"] != null)
            {
                ViewData["ErrorMessage"] = TempData["ErrorMessage"];
            }
            return View();
        }

        // POST: TeacherPage/Create
        [HttpPost]
        public IActionResult Create(Teacher NewTeacher)
        {

            string EmployeeNumberPattern = @"^T\d{3}$";

            // Check for the employee number pattern
            if (!string.IsNullOrEmpty(NewTeacher.EmployeeNumber) && !Regex.IsMatch(NewTeacher.EmployeeNumber, EmployeeNumberPattern))
            {
                TempData["ErrorMessage"] = "Employee number should start with 'T' followed by 3 digits. Eg: T123";
                return RedirectToAction("Validation");
            }
            // Check for the employee number which exist already
            if (!string.IsNullOrEmpty(NewTeacher.EmployeeNumber) && Regex.IsMatch(NewTeacher.EmployeeNumber, EmployeeNumberPattern))
            {
                List<Teacher> Teachers = _api.ListTeachers();
                foreach (Teacher CurrentTeacher in Teachers)
                {
                    if (CurrentTeacher.EmployeeNumber == NewTeacher.EmployeeNumber)
                    {
                        TempData["ErrorMessage"] = "This employee number has already been taken by the teacher";
                        return RedirectToAction("Validation");
                    }
                }
            }
            // Check for hire date
            if (string.IsNullOrEmpty(NewTeacher.HireDate))
            {
                TempData["ErrorMessage"] = "Hire Date cannot be empty.";
                return RedirectToAction("Validation");
            }
            // Check for future hire date
            if (!string.IsNullOrEmpty(NewTeacher.HireDate) && DateTime.Parse(NewTeacher.HireDate) > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Hire Date cannot be in future.";
                return RedirectToAction("Validation");
            }

            // Check for salary
            if (NewTeacher.Salary == 0)
            {
                TempData["ErrorMessage"] = "Salary cannot be empty.";
                return RedirectToAction("Validation");
            }

            // Check for salary less than 0
            if (NewTeacher.Salary < 0)
            {
                TempData["ErrorMessage"] = "Salary cannot be less than 0.";
                return RedirectToAction("Validation");
            }

            // Check for teacher name field from the input and respond with appropriate error message
            if (string.IsNullOrEmpty(NewTeacher.TeacherFName) && string.IsNullOrEmpty(NewTeacher.TeacherLName))
            {
                TempData["ErrorMessage"] = "Teacher first and last name cannot be empty";
                return RedirectToAction("Validation");
            }
            else if (string.IsNullOrEmpty(NewTeacher.TeacherFName))
            {
                TempData["ErrorMessage"] = "Teacher first name cannot be empty";
                return RedirectToAction("Validation");
            }
            else if (string.IsNullOrEmpty(NewTeacher.TeacherLName))
            {
                TempData["ErrorMessage"] = "Teacher last name cannot be empty";
                return RedirectToAction("Validation");
            }
            else
            {
                int TeacherId = _api.AddTeacher(NewTeacher);

                // redirects to "Show" action on "Teacher" cotroller with id parameter supplied
                return RedirectToAction("Show", new { id = TeacherId });
            }


        }

        // GET : TeacherPage/DeleteConfirm/{id}
        [HttpGet]
        public IActionResult DeleteConfirm(int id)
        {
            Teacher SelectedTeacher = _api.FindTeacher(id);
            return View(SelectedTeacher);
        }

        // POST: TeacherPage/Delete/{id}
        [HttpPost]
        public IActionResult Delete(int id)
        {
            string RowsAffected = _api.DeleteTeacher(id);

            // redirects to list action
            return RedirectToAction("List");
        }

        // GET : TeacherPage/Edit/{id}
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Teacher SelectedTeacher = _api.FindTeacher(id);
            ViewData["Id"] = id;
            return View(SelectedTeacher);

        }

        // POST: TeacherPage/Update/{id}
        [HttpPost]
        public IActionResult Update(int id, string TeacherFName, string TeacherLName, string EmployeeNumber, string HireDate, Decimal Salary)
        {
            string EmployeeNumberPattern = @"^T\d{3}$";
            Teacher UpdateTeacher = new Teacher();

            UpdateTeacher.TeacherFName = TeacherFName;
            UpdateTeacher.TeacherLName = TeacherLName;
            UpdateTeacher.EmployeeNumber = EmployeeNumber;
            UpdateTeacher.HireDate = HireDate;
            UpdateTeacher.Salary = Salary;

            // Check for the employee number validation
            if (string.IsNullOrEmpty(UpdateTeacher.EmployeeNumber))
            {
                TempData["ErrorMessage"] = "Employee number cannot be empty";
                return RedirectToAction("Validation");
            }

            // Check for the employee number pattern
            if (!string.IsNullOrEmpty(UpdateTeacher.EmployeeNumber) && !Regex.IsMatch(UpdateTeacher.EmployeeNumber, EmployeeNumberPattern))
            {
                TempData["ErrorMessage"] = "Employee number should start with 'T' followed by 3 digits. Eg: T123";
                return RedirectToAction("Validation");
            }
            // Check for the employee number which exist already
            if (!string.IsNullOrEmpty(UpdateTeacher.EmployeeNumber) && Regex.IsMatch(UpdateTeacher.EmployeeNumber, EmployeeNumberPattern))
            {
                List<Teacher> Teachers = _api.ListTeachers();
                foreach (Teacher CurrentTeacher in Teachers)
                {
                    if (UpdateTeacher.TeacherId == null && CurrentTeacher.EmployeeNumber == UpdateTeacher.EmployeeNumber)
                    {
                        TempData["ErrorMessage"] = "This employee number has already been taken by the teacher";
                        return RedirectToAction("Validation");
                    }
                }
            }

            // Check for hire date
            if (string.IsNullOrEmpty(UpdateTeacher.HireDate))
            {
                TempData["ErrorMessage"] = "Hire Date cannot be empty.";
                return RedirectToAction("Validation");
            }

            // Check for future hire date
            if (!string.IsNullOrEmpty(UpdateTeacher.HireDate) && DateTime.Parse(UpdateTeacher.HireDate) > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Hire Date cannot be in future.";
                return RedirectToAction("Validation");
            }

            // Check for salary
            if (UpdateTeacher.Salary == 0)
            {
                TempData["ErrorMessage"] = "Salary cannot be empty.";
                return RedirectToAction("Validation");
            }

            // Check for salary less than 0
            if (UpdateTeacher.Salary < 0)
            {
                TempData["ErrorMessage"] = "Salary cannot be less than 0.";
                return RedirectToAction("Validation");
            }


            // Check for teacher name field from the input and respond with appropriate error message
            if (string.IsNullOrEmpty(UpdateTeacher.TeacherFName) && string.IsNullOrEmpty(UpdateTeacher.TeacherLName))
            {
                TempData["ErrorMessage"] = "Teacher first and last name cannot be empty";
                return RedirectToAction("Validation");
            }
            else if (string.IsNullOrEmpty(UpdateTeacher.TeacherFName))
            {
                TempData["ErrorMessage"] = "Teacher first name cannot be empty";
                return RedirectToAction("Validation");
            }
            else if (string.IsNullOrEmpty(UpdateTeacher.TeacherLName))
            {
                TempData["ErrorMessage"] = "Teacher last name cannot be empty";
                return RedirectToAction("Validation");
            }


            else
            {
                _api.UpdateTeacher(id, UpdateTeacher);

                // redirects to show teacher
                return RedirectToAction("Show", new { id = id });
            }

        }

    }
}
