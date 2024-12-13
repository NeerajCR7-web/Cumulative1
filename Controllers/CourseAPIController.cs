using Cumulative1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
namespace Cumulative1.Controllers
{
    [Route("api/Course")]
    [ApiController]
    public class CourseAPIController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        // Dependency injection of school database context
        public CourseAPIController(SchoolDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Fetches a list of all courses in the database.
        /// </summary>
        /// <example>
        /// GET api/Course/ListCourses -> [{"courseId":1,"courseCode":"http5101","teacherId":1,"startDate":"2018-09-04","finishDate":"2018-12-14","courseName":"Web Application Development"},{"courseId":2,"courseCode":"http5102","teacherId":2,"startDate":"2018-09-04","finishDate":"2018-12-14","courseName":"Project Management"},{"courseId":3,"courseCode":"http5103","teacherId":5,"startDate":"2018-09-04","finishDate":"2018-12-14","courseName":"Web Programming"},..]
        /// </example>
        /// <returns>
        /// A list containing details of all courses.
        /// </returns>
        [HttpGet]
        [Route(template: "ListCourses")]
        public List<Course> ListCourses()
        {
            /// Initialize an empty list to store course information
            List<Course> Courses = new List<Course>();

            /// Use 'using' to ensure the database connection is closed after execution
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Establish a connection to the database
                Connection.Open();

                // Create a new database command for the query
                MySqlCommand Command = Connection.CreateCommand();

                // Define the SQL query to retrieve all courses
                Command.CommandText = "SELECT * FROM courses";

                // Execute the query and store the results
                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    // Process each row in the result set
                    while (ResultSet.Read())
                    {
                        Course CurrentCourse = new Course();
                        // Access Column information by the DB column name as an index
                        CurrentCourse.CourseId = Convert.ToInt32(ResultSet["courseid"]);
                        CurrentCourse.CourseCode = (ResultSet["coursecode"]).ToString();
                        CurrentCourse.TeacherId = Convert.ToInt32(ResultSet["teacherid"]);
                        CurrentCourse.StartDate = Convert.ToDateTime(ResultSet["startdate"]).ToString("yyyy-MM-dd");
                        CurrentCourse.FinishDate = Convert.ToDateTime(ResultSet["finishdate"]).ToString("yyyy-MM-dd");
                        CurrentCourse.CourseName = (ResultSet["coursename"]).ToString();
                        // Add it to the Courses list
                        Courses.Add(CurrentCourse);
                    }
                }
            }

            // Return the final list of courses
            return Courses;
        }
        /// <summary>
        /// Fetches details of a specific course by its ID.
        /// </summary>
        /// <param name="id">The ID of the course to be retrieved.</param>
        /// <example>
        /// GET api/Course/FindCourse/7 -> {"courseId":7,"courseCode":"http5202","teacherId":3,"startDate":"2019-01-08","finishDate":"2019-04-27","courseName":"Web Application Development 2"}
        /// </example>
        /// <returns>
        /// A course object matching the given ID or an empty object if no match is found.
        /// </returns>
        [HttpGet]
        [Route(template: "FindCourse/{id}")]
        public Course FindCourse(int id)
        {
            // Initialize an empty course object
            Course SelectedCourse = new Course();

            // Use 'using' to ensure the database connection is closed after execution
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Establish a connection to the database
                Connection.Open();

                // Create a new database command for the query
                MySqlCommand Command = Connection.CreateCommand();

                // Define the SQL query to retrieve a course by its ID
                Command.CommandText = "SELECT * FROM courses WHERE courseid=@id";
                Command.Parameters.AddWithValue("@id", id);

                // Execute the query and store the results
                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    // Process each row in the result set
                    while (ResultSet.Read())
                    {
                        // Extract column data using the column name as a reference
                        SelectedCourse.CourseId = Convert.ToInt32(ResultSet["courseid"]);
                        SelectedCourse.CourseCode = (ResultSet["coursecode"]).ToString();
                        SelectedCourse.TeacherId = Convert.ToInt32(ResultSet["teacherid"]);
                        SelectedCourse.StartDate = Convert.ToDateTime(ResultSet["startdate"]).ToString("yyyy-MM-dd");
                        SelectedCourse.FinishDate = Convert.ToDateTime(ResultSet["finishdate"]).ToString("yyyy-MM-dd");
                        SelectedCourse.CourseName = (ResultSet["coursename"]).ToString();
                    }
                }
            }

            // Return the course object with the matching ID
            return SelectedCourse;
        }
        /// <summary>
        /// Adds a new course to the database.
        /// </summary>
        /// <param name="CourseData">An object containing the course details to be added.</param>
        /// <returns>
        /// The ID of the newly added course if the operation is successful; otherwise, returns 0.
        /// </returns>
        [HttpPost(template: "AddCourse")]
        public int AddCourse([FromBody] Course CourseData)
        {
            // Establish a connection to the database using the context's AccessDatabase method.
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the database connection.
                Connection.Open();

                // Create a new SQL command to execute the insert query.
                MySqlCommand Command = Connection.CreateCommand();
                // Define the SQL query to insert a new course with placeholders for parameterized values.
                Command.CommandText = "INSERT INTO courses(coursecode,teacherid,startdate,finishdate,coursename) VALUES(@coursecode,@teacherid,@startdate,@finishdate,@coursename)";

                // Add parameters to the SQL command, binding each field in the Course object to the respective placeholder.
                Command.Parameters.AddWithValue("@coursecode", CourseData.CourseCode);
                Command.Parameters.AddWithValue("@teacherid", CourseData.TeacherId);
                Command.Parameters.AddWithValue("@startdate", CourseData.StartDate);
                Command.Parameters.AddWithValue("@finishdate", CourseData.FinishDate);
                Command.Parameters.AddWithValue("@coursename", CourseData.CourseName);

                // Execute the SQL command to insert the course into the database.
                Command.ExecuteNonQuery();
                // Retrieve and return the unique ID of the newly inserted course record.
                return Convert.ToInt32(Command.LastInsertedId);
            }
            return 0;
        }

        /// <summary>
        /// Deletes a course from the database based on the provided course ID.
        /// </summary>
        /// <param name="CourseId">The ID of the course to be deleted.</param>
        /// <returns>
        /// A message indicating whether the course was successfully removed or if it was not found.
        /// </returns>
        [HttpDelete(template: "DeleteCourse/{CourseId}")]
        public string DeleteCourse(int CourseId)
        {
            // Tracks the number of rows affected by the deletion operation.
            int RowsAffected = 0;

            // Establish a connection to the database using the context's AccessDatabase method.
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection.
                Connection.Open();
                // Create a SQL command to delete a course based on its ID.
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "DELETE FROM courses WHERE courseid=@id";
                // Add a parameter to the SQL command to prevent SQL injection.
                Command.Parameters.AddWithValue("@id", CourseId);
                // Execute the command and get the number of affected rows.
                RowsAffected = Command.ExecuteNonQuery();
            }
            // Check if any rows were affected by the delete operation.
            if (RowsAffected > 0)
            {
                // Return a message confirming the course was successfully deleted.
                return $"The course with ID {CourseId} has been successfully removed from the database.";
            }
            else
            {
                // Return a message indicating no course was found with the given ID.
                return $"No course found with ID {CourseId}.";
            }
        }
        /// <summary>
        /// Modifies the details of an existing Course in the database. 
        /// The Course object contains the updated data, and the Course ID is provided in the request.
        /// </summary>
        /// <param name="CourseData">The Course object containing updated details</param>
        /// <param name="CourseId">The ID of the Course to be updated</param>
        /// <example>
        /// PUT: api/Course/UpdateCourse/4
        /// Headers: Content-Type: application/json
        /// Request Body:
        /// {
        ///	    "CourseCode":"science&technology 121",
        ///	    "TeacherId":"4",
        ///	    "StartDate":"2023-01-02 00:00:00",
        ///	    "FinishDate":"2023-08-02 00:00:00",
        ///	    "CourseName":"nanotechnology"
        /// }
        /// Response:
        /// {
        ///     "CourseId":4,
        ///	     "CourseCode":"science&technology 121",
        ///	    "TeacherId":"4",
        ///	    "StartDate":"2023-01-02 00:00:00",
        ///	    "FinishDate":"2023-08-02 00:00:00",
        ///	    "CourseName":"nanotechnology"
        /// }
        /// </example>
        /// <returns>
        /// Returns the updated Course object with the latest details.
        /// </returns>

        [HttpPut(template: "UpdateCourse/{CourseId}")]
        public Course UpdateCourse(int CourseId, [FromBody] Course CourseData)
        {
            // 'using' will close the connection after the code executes
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection
                Connection.Open();

                // Establish a new command (query) for our database
                MySqlCommand Command = Connection.CreateCommand();

                Command.CommandText = "UPDATE courses SET coursecode=@coursecode, teacherid=@teacherid, startdate=@startdate, finishdate=@finishdate, coursename=@coursename WHERE courseid=@id";
                Command.Parameters.AddWithValue("@coursecode", CourseData.CourseCode);
                Command.Parameters.AddWithValue("@teacherid", CourseData.TeacherId);
                Command.Parameters.AddWithValue("@startdate", CourseData.StartDate);
                Command.Parameters.AddWithValue("@finishdate", CourseData.FinishDate);
                Command.Parameters.AddWithValue("@coursename", CourseData.CourseName);
                Command.Parameters.AddWithValue("@id", CourseId);

                Command.ExecuteNonQuery();
            }

            return FindCourse(CourseId);
        }
    }
}
