using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Cumulative1.Models;

namespace Cumulative1.Controllers
{
    [Route("api/Course")]
    [ApiController]
    public class CourseAPIController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public CourseAPIController(SchoolDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all courses.
        /// </summary>
        /// <returns>A list of all courses in the database.</returns>
        [HttpGet("ListCourses")]
        public IActionResult ListCourses()
        {
            var courses = new List<Course>();

            using (var connection = _context.AccessDatabase())
            {
                connection.Open();
                var command = new MySqlCommand("SELECT * FROM courses", connection);
                using (var resultSet = command.ExecuteReader())
                {
                    while (resultSet.Read())
                    {
                        courses.Add(new Course
                        {
                            CourseId = Convert.ToInt32(resultSet["courseid"]),
                            CourseCode = resultSet["coursecode"].ToString(),
                            TeacherId = Convert.ToInt32(resultSet["teacherid"]),
                            StartDate = Convert.ToDateTime(resultSet["startdate"]).ToString("yyyy-MM-dd"),
                            FinishDate = Convert.ToDateTime(resultSet["finishdate"]).ToString("yyyy-MM-dd"),
                            CourseName = resultSet["coursename"].ToString()
                        });
                    }
                }
            }

            return Ok(courses);
        }

        /// <summary>
        /// Retrieves a course by its ID.
        /// </summary>
        /// <param name="id">Course ID</param>
        /// <returns>The course details if found; otherwise, a not found response.</returns>
        [HttpGet("FindCourse/{id}")]
        public IActionResult FindCourse(int id)
        {
            Course course = null;

            using (var connection = _context.AccessDatabase())
            {
                connection.Open();
                var command = new MySqlCommand("SELECT * FROM courses WHERE courseid=@id", connection);
                command.Parameters.AddWithValue("@id", id);
                using (var resultSet = command.ExecuteReader())
                {
                    if (resultSet.Read())
                    {
                        course = new Course
                        {
                            CourseId = Convert.ToInt32(resultSet["courseid"]),
                            CourseCode = resultSet["coursecode"].ToString(),
                            TeacherId = Convert.ToInt32(resultSet["teacherid"]),
                            StartDate = Convert.ToDateTime(resultSet["startdate"]).ToString("yyyy-MM-dd"),
                            FinishDate = Convert.ToDateTime(resultSet["finishdate"]).ToString("yyyy-MM-dd"),
                            CourseName = resultSet["coursename"].ToString()
                        };
                    }
                }
            }

            if (course == null)
                return NotFound($"Course with ID {id} not found.");
            return Ok(course);
        }

        /// <summary>
        /// Adds a new course.
        /// </summary>
        /// <param name="course">Course data</param>
        /// <returns>The ID of the newly created course.</returns>
        [HttpPost("AddCourse")]
        public IActionResult AddCourse([FromBody] Course course)
        {
            using (var connection = _context.AccessDatabase())
            {
                connection.Open();
                var command = new MySqlCommand(
                    "INSERT INTO courses(coursecode,teacherid,startdate,finishdate,coursename) VALUES(@coursecode,@teacherid,@startdate,@finishdate,@coursename)",
                    connection);
                command.Parameters.AddWithValue("@coursecode", course.CourseCode);
                command.Parameters.AddWithValue("@teacherid", course.TeacherId);
                command.Parameters.AddWithValue("@startdate", course.StartDate);
                command.Parameters.AddWithValue("@finishdate", course.FinishDate);
                command.Parameters.AddWithValue("@coursename", course.CourseName);
                command.ExecuteNonQuery();

                return CreatedAtAction(nameof(FindCourse), new { id = command.LastInsertedId }, course);
            }
        }

        /// <summary>
        /// Deletes a course by ID.
        /// </summary>
        /// <param name="id">Course ID</param>
        /// <returns>A message indicating success or failure.</returns>
        [HttpDelete("DeleteCourse/{id}")]
        public IActionResult DeleteCourse(int id)
        {
            int rowsAffected;
            using (var connection = _context.AccessDatabase())
            {
                connection.Open();
                var command = new MySqlCommand("DELETE FROM courses WHERE courseid=@id", connection);
                command.Parameters.AddWithValue("@id", id);
                rowsAffected = command.ExecuteNonQuery();
            }

            if (rowsAffected > 0)
                return Ok($"Course with ID {id} has been deleted.");
            return NotFound($"Course with ID {id} not found.");
        }
    }
}
