using Cumulative1.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Cumulative1.Models;

namespace Cumulative1.Controllers
{
    [Route("api/Student")]
    [ApiController]
    public class StudentAPIController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        // Dependency injection of school database context
        public StudentAPIController(SchoolDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of Students in the system
        /// </summary>
        /// <example>
        /// GET api/Student/ListStudents -> [{"studentId": 101,"studentFName": "John","studentLName": "Doe","studentNumber": "N1701","enrolDate": "2019-09-15"},{"studentId": 102,"studentFName": "Emma","studentLName": "Clark","studentNumber": "N1702","enrolDate": "2020-01-10" },{ "studentId": 103,"studentFName": "Liam","studentLName": "Smith","studentNumber": "N1703","enrolDate": "2019-11-20"}]

        /// </example>
        /// <returns>
        /// A list of student objects 
        /// </returns>
        [HttpGet]
        [Route(template: "ListStudents")]
        public List<Student> ListStudents()
        {
            // Create an empty list of Students
            List<Student> Students = new List<Student>();

            // 'using' will close the connection after the code executes
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection
                Connection.Open();

                // Establish a new command (query) for our database
                MySqlCommand Command = Connection.CreateCommand();

                // Set the SQL Query
                Command.CommandText = "SELECT * FROM students";

                // Gather Result Set of Query into a variable
                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    // Loop Through Each Row of the Result Set
                    while (ResultSet.Read())
                    {
                        Student CurrentStudent = new Student();
                        // Access Column information by the DB column name as an index
                        CurrentStudent.StudentId = Convert.ToInt32(ResultSet["studentid"]);
                        CurrentStudent.StudentFName = (ResultSet["studentfname"]).ToString();
                        CurrentStudent.StudentLName = (ResultSet["studentlname"]).ToString();
                        CurrentStudent.StudentNumber = (ResultSet["studentnumber"]).ToString();
                        CurrentStudent.EnrolDate = ResultSet["enroldate"] != DBNull.Value ? Convert.ToDateTime(ResultSet["enroldate"]).ToString("yyyy/MM/dd") : "";
                        // Add it to the Students list
                        Students.Add(CurrentStudent);
                    }
                }
            }
            // Return the final list of students
            return Students;
        }


        /// <summary>
        /// Returns a student in the database by their ID
        /// </summary>
        /// <param name="id">It accepts an id which is an integer</param>
        /// <example>
        /// GET api/Student/FindStudent/7 -> {"studentId":7,"studentFName":"Jason","studentLName":"Freeman","studentNumber":"N1694","enrolDate":"2018-08-16"}
        /// </example>
        /// <returns>
        /// A matching student object by its ID. Empty object if Student not found
        /// </returns>
        [HttpGet]
        [Route(template: "FindStudent/{id}")]
        public Student FindStudent(int id)
        {
            //Empty Student
            Student SelectedStudent = new Student();

            // 'using' will close the connection after the code executes
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection
                Connection.Open();

                // Establish a new command (query) for our database
                MySqlCommand Command = Connection.CreateCommand();

                // Set the SQL Query
                Command.CommandText = "SELECT * FROM students WHERE studentid=@id";
                Command.Parameters.AddWithValue("@id", id);

                // Gather Result Set of Query into a variable
                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    // Loop Through Each Row of the Result Set
                    while (ResultSet.Read())
                    {
                        // Access Column information by the DB column name as an index
                        SelectedStudent.StudentId = Convert.ToInt32(ResultSet["studentid"]);
                        SelectedStudent.StudentFName = (ResultSet["studentfname"]).ToString();
                        SelectedStudent.StudentLName = (ResultSet["studentlname"]).ToString();
                        SelectedStudent.StudentNumber = (ResultSet["studentnumber"]).ToString();
                        SelectedStudent.EnrolDate = ResultSet["enroldate"] != DBNull.Value ? Convert.ToDateTime(ResultSet["enroldate"]).ToString("yyyy/MM/dd") : "";

                    }
                }
            }
            // Return the final list of student names
            return SelectedStudent;
        }


        /// curl -X "POST" -H "Content-Type: application/json" -d "{\"studentFName\": \"Jane\",\"studentLName\": \"Williams\",\"studentNumber\": \"N7879\",\"enrolDate\": \"2019-01-15\"}" "https://localhost:7121/api/Student/AddStudent"

        /// <summary>
        /// Inserts a new student record into the database.
        /// </summary>
        /// <param name="StudentData">An object containing the student's details.</param>
        /// <example>
        /// POST: api/Student/AddStudent  
        /// Headers: Content-Type: application/json  
        /// Request Body:  
        /// {  
        ///   "StudentFname": "Emma",  
        ///   "StudentLname": "Johnson",  
        ///   "StudentNumber": "N5678",  
        ///   "EnrolDate": "09-01-2023"  
        /// } -> 42
        /// </example>
        /// <returns>
        /// The ID of the newly added student if the operation succeeds; otherwise, returns 0.
        /// </returns>


        [HttpPost(template: "AddStudent")]
        public int AddStudent([FromBody] Student StudentData)
        {
            // 'using' will close the connection after the code executes
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection
                Connection.Open();

                // Establish a new command (query) for our database
                MySqlCommand Command = Connection.CreateCommand();

                // Set the SQL Command
                Command.CommandText = "INSERT INTO students (studentfname,studentlname,studentnumber,enroldate) VALUES (@studentfname,@studentlname,@studentnumber,@enroldate)";

                Command.Parameters.AddWithValue("@studentfname", StudentData.StudentFName);
                Command.Parameters.AddWithValue("@studentlname", StudentData.StudentLName);
                Command.Parameters.AddWithValue("@studentnumber", StudentData.StudentNumber);
                Command.Parameters.AddWithValue("@enroldate", StudentData.EnrolDate);

                Command.ExecuteNonQuery();

                // Send the last inserted id of the data created
                return Convert.ToInt32(Command.LastInsertedId);

            }

            // if failure
            return 0;
        }

        /// curl -X "DELETE" "https://localhost:7121/api/Student/DeleteStudent/33"

        /// <summary>
        /// Removes a student record from the database.
        /// </summary>
        /// <param name="StudentId">The unique identifier of the student to be removed.</param>
        /// <example>
        /// DELETE: api/Student/DeleteStudent/{StudentId} -> "The student with given id {StudentId} has been removed from the database."
        /// </example>
        /// <returns>
        /// A message indicating the operation's outcome:  
        /// - If the student ID exists, it returns: "The student with given id {studentid} has been removed from the database."  
        /// - If the student ID does not exist, it returns: "The student with given id {studentid} is not found."
        /// </returns>


        [HttpDelete(template: "DeleteStudent/{StudentId}")]
        public string DeleteStudent(int StudentId)
        {
            // initialize the variable to track the rows affected
            int RowsAffected = 0;

            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection
                Connection.Open();

                // Establish a new command (query) for our database
                MySqlCommand Command = Connection.CreateCommand();

                // Set the SQL Command
                Command.CommandText = "DELETE FROM students WHERE studentid=@id";

                Command.Parameters.AddWithValue("@id", StudentId);

                RowsAffected = Command.ExecuteNonQuery();

            }
            // Check for the deletion
            if (RowsAffected > 0)
            {
                return $"The student with ID {StudentId} has been successfully deleted from the database.";
            }
            else
            {
                return $"No student found with ID {StudentId}.";
            }

        }
        /// <summary>
        /// Updates the details of a specific Student in the database. 
        /// The updated data is provided as a Student object, and the Student ID is included in the request path.
        /// </summary>
        /// <param name="StudentData">The Student object containing updated information</param>
        /// <param name="StudentId">The unique ID of the Student to be updated</param>
        /// <example>
        /// PUT: api/Student/UpdateStudent/4
        /// Headers: Content-Type: application/json
        /// Request Body:
        /// {
        ///     "StudentFName": "aniket",
        ///     "StudentLName": "sharma",
        ///     "StudentNumber": "T321",
        ///     "EnrolDate": "2023-05-07 00:00:00"
        /// }
        /// Response:
        /// {
        ///     "StudentId": 7,
        ///     "StudentFName": "aniket",
        ///     "StudentLName": "sharma",
        ///     "StudentNumber": "T321",
        ///     "EnrolDate": "2023-05-07 00:00:00"
        /// }
        /// </example>
        /// <returns>
        /// Returns the updated Student object with the latest data from the database.
        /// </returns>
        [HttpPut(template: "UpdateStudent/{StudentId}")]
        public Student UpdateStudent(int StudentId, [FromBody] Student StudentData)
        {
            // 'using' will close the connection after the code executes
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                // Open the connection
                Connection.Open();

                // Establish a new command (query) for our database
                MySqlCommand Command = Connection.CreateCommand();

                Command.CommandText = "UPDATE students SET studentfname=@studentfname, studentlname=@studentlname, studentnumber=@studentnumber, enroldate=@enroldate WHERE studentid=@id";
                Command.Parameters.AddWithValue("@studentfname", StudentData.StudentFName);
                Command.Parameters.AddWithValue("@studentlname", StudentData.StudentLName);
                Command.Parameters.AddWithValue("@studentnumber", StudentData.StudentNumber);
                Command.Parameters.AddWithValue("@enroldate", StudentData.EnrolDate);
                Command.Parameters.AddWithValue("@id", StudentId);

                Command.ExecuteNonQuery();
            }

            return FindStudent(StudentId);
        }
    }
}