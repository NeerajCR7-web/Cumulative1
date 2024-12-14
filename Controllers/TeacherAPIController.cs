
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Cumulative1.Models;


namespace Cumulative1.Controllers
{
    [Route("api/Teacher")]
    [ApiController]
    public class TeacherAPIController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public TeacherAPIController(SchoolDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Returns a list of Teachers in the system
        /// </summary>
        /// <example>
        /// GET api/Teacher/ListTeachers -> [{"teacherId":1,"teacherFName":"Alexander","teacherLName":"Bennett","employeeNumber":"T378","hireDate":"2016-08-05 00:00:00","salary":55.30,"coursesByTeacher":[{"courseId":1,"courseCode":"http5101","teacherId":1,"startDate":"2018-09-04","finishDate":"2018-12-14","courseName":"Web Application Development"}]},{"teacherId":2,"teacherFName":"Caitlin","teacherLName":"Cummings","employeeNumber":"T381","hireDate":"2014-06-10 00:00:00","salary":62.77,"coursesByTeacher":[{"courseId":2,"courseCode":"http5102","teacherId":2,"startDate":"2018-09-04","finishDate":"2018-12-14","courseName":"Project Management"},{"courseId":6,"courseCode":"http5201","teacherId":2,"startDate":"2019-01-08","finishDate":"2019-04-27","courseName":"Security & Quality Assurance"}]},..]
        /// </example>
        /// <returns>
        /// A list of teacher objects 
        /// </returns>
        [HttpGet]
        [Route(template: "ListTeachers")]
        public List<Teacher> ListTeachers()
        {

            // Create an empty list of Teachers
            List<Teacher> Teachers = new List<Teacher>();

            // 'using' will close the connection after the code executes
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection
                Connection.Open();

                // Establish a new command (query) for our database
                MySqlCommand Command = Connection.CreateCommand();

                // Set SQL Query
                Command.CommandText = "SELECT * FROM teachers";

                // Gather Result Set of Query into a variable
                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    // Loop Through Each Row of the Result Set
                    while (ResultSet.Read())
                    {
                        Teacher CurrentTeacher = new Teacher();
                        List<Course> Courses = new List<Course>();
                        // Access Column information by the DB column name as an index
                        CurrentTeacher.TeacherId = Convert.ToInt32(ResultSet["teacherid"]);
                        CurrentTeacher.TeacherFName = ResultSet["teacherfname"].ToString();
                        CurrentTeacher.TeacherLName = ResultSet["teacherlname"].ToString();
                        CurrentTeacher.EmployeeNumber = ResultSet["employeenumber"].ToString();
                        CurrentTeacher.HireDate = ResultSet["hiredate"] != DBNull.Value ? Convert.ToDateTime(ResultSet["hiredate"]).ToString("yyyy/MM/dd HH:mm:ss") : "";
                        CurrentTeacher.Salary = Convert.ToDecimal(ResultSet["salary"]);
                        foreach (Course CourseDetails in ListCourses())
                        {
                            if (CurrentTeacher.TeacherId == CourseDetails.TeacherId)
                            {

                                Courses.Add(CourseDetails);
                            }
                        }
                        CurrentTeacher.CoursesByTeacher = Courses;
                        // Add it to the Teachers list
                        Teachers.Add(CurrentTeacher);
                    }

                }

            }

            // Return the final list of teachers
            return Teachers;
        }


        /// <summary>
        /// Retrieves a list of all courses in the system.
        /// </summary>
        /// <example>
        /// GET api/Course/ListCourses -> [{"courseId":1,"courseCode":"http5101","teacherId":1,"startDate":"2018-09-04","finishDate":"2018-12-14","courseName":"Web Application Development"},{"courseId":2,"courseCode":"http5102","teacherId":2,"startDate":"2018-09-04","finishDate":"2018-12-14","courseName":"Project Management"},{"courseId":3,"courseCode":"http5103","teacherId":5,"startDate":"2018-09-04","finishDate":"2018-12-14","courseName":"Web Programming"},..]
        /// </example>
        /// <returns>
        /// A list of course objects.
        /// </returns>
        [HttpGet]
        [Route(template: "ListCourses")]
        public List<Course> ListCourses()
        {
            // Initialize an empty list to store course details
            List<Course> Courses = new List<Course>();

            // Ensure the database connection is closed after the execution
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection to the database
                Connection.Open();

                // Create a command object to execute the query
                MySqlCommand Command = Connection.CreateCommand();

                // Define the SQL query to fetch all courses
                Command.CommandText = "SELECT * FROM courses";

                // Execute the query and store the result set
                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    // Process each record in the result set
                    while (ResultSet.Read())
                    {
                        Course CurrentCourse = new Course();
                        // Extract column values based on column names
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
        /// Fetches details of a specific teacher by their ID.
        /// </summary>
        /// <param name="id">The ID of the teacher to retrieve.</param>
        /// <example>
        /// GET api/Teacher/FindTeacher/7 -> {"teacherId":7,"teacherFName":"Shannon","teacherLName":"Barton","employeeNumber":"T397","hireDate":"2013-08-04 00:00:00","salary":64.70,"coursesByTeacher":[{"courseId":4,"courseCode":"http5104","teacherId":7,"startDate":"2018-09-04","finishDate":"2018-12-14","courseName":"Digital Design"}]}
        /// </example>
        /// <returns>
        /// The teacher object matching the given ID or an empty object if not found.
        /// </returns>
        [HttpGet]
        [Route(template: "FindTeacher/{id}")]
        public Teacher FindTeacher(int id)
        {
            // Initialize an empty teacher object
            Teacher SelectedTeacher = new Teacher();
            List<Course> Courses = new List<Course>();
            // Ensure the database connection is closed after the execution
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection to the database
                Connection.Open();

                // Create a command object to execute the query
                MySqlCommand Command = Connection.CreateCommand();

                // Define the SQL query to fetch the teacher by ID
                Command.CommandText = "SELECT * FROM teachers WHERE teacherid=@id";
                Command.Parameters.AddWithValue("@id", id);

                // Execute the query and store the result set
                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    // Process each record in the result set
                    while (ResultSet.Read())
                    {
                        // Extract column values based on column names
                        SelectedTeacher.TeacherId = Convert.ToInt32(ResultSet["teacherid"]);
                        SelectedTeacher.TeacherFName = ResultSet["teacherfname"].ToString();
                        SelectedTeacher.TeacherLName = ResultSet["teacherlname"].ToString();
                        SelectedTeacher.EmployeeNumber = ResultSet["employeenumber"].ToString();
                        SelectedTeacher.HireDate = ResultSet["hiredate"] != DBNull.Value ? Convert.ToDateTime(ResultSet["hiredate"]).ToString("yyyy/MM/dd HH:mm:ss") : "";
                        SelectedTeacher.Salary = Convert.ToDecimal(ResultSet["salary"]);

                        // Match courses taught by the teacher
                        foreach (Course CourseDetails in ListCourses())
                        {
                            if (SelectedTeacher.TeacherId == CourseDetails.TeacherId)
                            {

                                Courses.Add(CourseDetails);
                            }
                        }
                        SelectedTeacher.CoursesByTeacher = Courses;
                    }

                }

            }
            // Return the teacher object
            return SelectedTeacher;

        }



        /// curl -X "POST" -H "Content-Type: application/json" -d "{\"teacherFName\": \"Robert\", \"teacherLName\": \"Smith\", \"employeeNumber\": \"T102\", \"hireDate\": \"2024-11-22 00:00:00\", \"salary\": 55.25}" "https://localhost:7121/api/Teacher/AddTeacher"
        /// <summary>
        /// Adds a new teacher to the database.
        /// </summary>
        /// <param name="TeacherData">An object containing the teacher's details to be added.</param>
        /// <example>
        /// POST: api/Teacher/AddTeacher  
        /// Headers: Content-Type: application/json  
        /// Request Body:  
        /// {  
        ///   "TeacherFName": "John",  
        ///   "TeacherLName": "Doe",  
        ///   "EmployeeNumber": "T205",  
        ///   "HireDate": "2024-11-10",  
        ///   "Salary": 60.75  
        /// }  
        /// Returns: 101
        /// </example>
        /// <returns>
        /// Returns the unique ID of the newly added teacher if the operation is successful, or 0 if unsuccessful.
        /// </returns>


        [HttpPost(template: "AddTeacher")]
        public int AddTeacher([FromBody] Teacher TeacherData)

        {
            // 'using' will close the connection after the code executes
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection
                Connection.Open();

                // Establish a new command (query) for our database
                MySqlCommand Command = Connection.CreateCommand();

                // Set the SQL Command
                Command.CommandText = "INSERT INTO teachers (teacherfname,teacherlname,employeenumber,hiredate,salary) VALUES (@teacherfname,@teacherlname,@employeenumber,@hiredate,@salary)";
                Command.Parameters.AddWithValue("@teacherfname", TeacherData.TeacherFName);
                Command.Parameters.AddWithValue("@teacherlname", TeacherData.TeacherLName);
                Command.Parameters.AddWithValue("@employeenumber", TeacherData.EmployeeNumber);
                Command.Parameters.AddWithValue("@hiredate", TeacherData.HireDate);
                Command.Parameters.AddWithValue("@salary", TeacherData.Salary);


                Command.ExecuteNonQuery();


                // Send the last inserted id of the data created
                return Convert.ToInt32(Command.LastInsertedId);
            }

            // if failure
            return 0;
        }

        /// curl -X "DELETE" "https://localhost:7151/api/Teacher/DeleteTeacher/20"
        /// <summary>
        /// Deletes a teacher record from the database.
        /// </summary>
        /// <param name="TeacherId">The unique ID of the teacher to be deleted.</param>
        /// <example>
        /// DELETE: api/Teacher/DeleteTeacher/{TeacherId}  
        /// Response: "Teacher with ID 205 has been successfully deleted."
        /// </example>
        /// <returns>
        /// A message indicating whether the deletion was successful:  
        /// - If the teacher ID is found and deleted, it returns: "Teacher with ID {TeacherId} has been successfully deleted."  
        /// - If the teacher ID is not found, it returns: "No teacher found with ID {TeacherId}."
        /// </returns>

        [HttpDelete(template: "DeleteTeacher/{TeacherId}")]
        public string DeleteTeacher(int TeacherId)
        {
            // initialize the variable to track the rows affected
            int RowsAffected = 0;

            // 'using' will close the connection after the code executes
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection
                Connection.Open();

                // Establish a new command (query) for our database
                MySqlCommand Command = Connection.CreateCommand();

                // Set the SQL Command
                Command.CommandText = "DELETE FROM teachers WHERE teacherid=@id";
                Command.Parameters.AddWithValue("@id", TeacherId);

                RowsAffected = Command.ExecuteNonQuery();

            }
            // Check for the deletion
            if (RowsAffected > 0)
            {
                return $"The teacher with given id {TeacherId} has been removed from the DB";
            }
            else
            {
                return $"The teacher with given id {TeacherId} is not found";
            }

        }


        /// <summary>
        /// Updates the information of a specific Teacher in the database. 
        /// The Teacher object contains the updated details, and the Teacher ID is provided in the request path.
        /// </summary>
        /// <param name="TeacherData">The Teacher object with updated information</param>
        /// <param name="TeacherId">The unique ID of the Teacher to be updated</param>
        /// <example>
        /// PUT: api/Teacher/UpdateTeacher/4  
        /// Headers: Content-Type: application/json  
        /// Request Body:  
        /// {
        ///     "TeacherFname": "aniket",
        ///     "TeacherLname": "Sharma",
        ///     "EmployeeNumber": "T321",
        ///     "HireDate": "2023-05-05 00:00:00",
        ///     "Salary": "40.30"
        /// }  
        /// Response:  
        /// {
        ///     "TeacherId": 3,
        ///     "TeacherFname": "aniket",
        ///     "TeacherLname": "Sharma",
        ///     "EmployeeNumber": "T321",
        ///     "HireDate": "2023-05-05 00:00:00",
        ///     "Salary": "40.30"
        /// }
        /// </example>
        /// <returns>
        /// Returns the updated Teacher object with the latest data from the database.
        /// </returns>

        [HttpPut(template: "UpdateTeacher/{TeacherId}")]
        public Teacher UpdateTeacher(int TeacherId, [FromBody] Teacher TeacherData)
        {
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection
                Connection.Open();

                // Establish a new comman for  database
                MySqlCommand Command = Connection.CreateCommand();

                Command.CommandText = "UPDATE teachers SET teacherfname=@teacherfname, teacherlname=@teacherlname, employeenumber=@employeenumber, hiredate=@hiredate, salary=@salary where teacherid=@id";
                Command.Parameters.AddWithValue("@teacherfname", TeacherData.TeacherFName);
                Command.Parameters.AddWithValue("@teacherlname", TeacherData.TeacherLName);
                Command.Parameters.AddWithValue("@employeenumber", TeacherData.EmployeeNumber);
                Command.Parameters.AddWithValue("@hiredate", TeacherData.HireDate);
                Command.Parameters.AddWithValue("@salary", TeacherData.Salary);

                Command.Parameters.AddWithValue("@id", TeacherId);

                Command.ExecuteNonQuery();
            }


            return FindTeacher(TeacherId);
        }


    }

}