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
        /// Retrieves a list of all students in the database.
        /// </summary>
        /// <example>
        /// GET api/Student/ListStudents -> [{"studentId":1,"studentFName":"Sarah","studentLName":"Valdez","studentNumber":"N1678","enrolDate":"2018-06-18"},...]
        /// </example>
        /// <returns>A list of all students as objects.</returns>
        [HttpGet]
        [Route(template: "ListStudents")]
        public List<Student> ListStudents()
        {
            List<Student> Students = new List<Student>();

            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                Connection.Open();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "SELECT * FROM students";

                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    while (ResultSet.Read())
                    {
                        Student CurrentStudent = new Student
                        {
                            StudentId = Convert.ToInt32(ResultSet["studentid"]),
                            StudentFName = (ResultSet["studentfname"]).ToString(),
                            StudentLName = (ResultSet["studentlname"]).ToString(),
                            StudentNumber = (ResultSet["studentnumber"]).ToString(),
                            EnrolDate = ResultSet["enroldate"] != DBNull.Value ? Convert.ToDateTime(ResultSet["enroldate"]).ToString("yyyy/MM/dd") : ""
                        };
                        Students.Add(CurrentStudent);
                    }
                }
            }
            return Students;
        }

        /// <summary>
        /// Finds and retrieves a student from the database by their ID.
        /// </summary>
        /// <param name="id">The ID of the student to retrieve.</param>
        /// <example>
        /// GET api/Student/FindStudent/7 -> {"studentId":7,"studentFName":"Jason","studentLName":"Freeman","studentNumber":"N1694","enrolDate":"2018-08-16"}
        /// </example>
        /// <returns>The matching student object, or an empty object if no match is found.</returns>
        [HttpGet]
        [Route(template: "FindStudent/{id}")]
        public Student FindStudent(int id)
        {
            Student SelectedStudent = new Student();

            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                Connection.Open();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "SELECT * FROM students WHERE studentid=@id";
                Command.Parameters.AddWithValue("@id", id);

                using (MySqlDataReader ResultSet = Command.ExecuteReader())
                {
                    while (ResultSet.Read())
                    {
                        SelectedStudent.StudentId = Convert.ToInt32(ResultSet["studentid"]);
                        SelectedStudent.StudentFName = (ResultSet["studentfname"]).ToString();
                        SelectedStudent.StudentLName = (ResultSet["studentlname"]).ToString();
                        SelectedStudent.StudentNumber = (ResultSet["studentnumber"]).ToString();
                        SelectedStudent.EnrolDate = ResultSet["enroldate"] != DBNull.Value ? Convert.ToDateTime(ResultSet["enroldate"]).ToString("yyyy/MM/dd") : "";
                    }
                }
            }
            return SelectedStudent;
        }

        /// <summary>
        /// Adds a new student to the database.
        /// </summary>
        /// <param name="StudentData">The student object to be added.</param>
        /// <example>
        /// POST api/Student/AddStudent -> 25 (ID of the newly added student).
        /// </example>
        /// <returns>The ID of the inserted student, or 0 if the operation fails.</returns>
        [HttpPost(template: "AddStudent")]
        public int AddStudent([FromBody] Student StudentData)
        {
            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                Connection.Open();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "INSERT INTO students (studentfname,studentlname,studentnumber,enroldate) VALUES (@studentfname,@studentlname,@studentnumber,@enroldate)";

                Command.Parameters.AddWithValue("@studentfname", StudentData.StudentFName);
                Command.Parameters.AddWithValue("@studentlname", StudentData.StudentLName);
                Command.Parameters.AddWithValue("@studentnumber", StudentData.StudentNumber);
                Command.Parameters.AddWithValue("@enroldate", StudentData.EnrolDate);

                Command.ExecuteNonQuery();

                return Convert.ToInt32(Command.LastInsertedId);
            }
            return 0;
        }

        /// <summary>
        /// Deletes a student from the database by their ID.
        /// </summary>
        /// <param name="StudentId">The ID of the student to delete.</param>
        /// <example>
        /// DELETE api/Student/DeleteStudent/33 -> "The student with given id 33 has been removed from the DB".
        /// </example>
        /// <returns>A message indicating success or failure of the deletion.</returns>
        [HttpDelete(template: "DeleteStudent/{StudentId}")]
        public string DeleteStudent(int StudentId)
        {
            int RowsAffected = 0;

            using (MySqlConnection Connection = _context.AccessDatabase())
            {
                Connection.Open();
                MySqlCommand Command = Connection.CreateCommand();
                Command.CommandText = "DELETE FROM students WHERE studentid=@id";
                Command.Parameters.AddWithValue("@id", StudentId);

                RowsAffected = Command.ExecuteNonQuery();
            }

            return RowsAffected > 0
                ? $"The student with given id {StudentId} has been removed from the DB"
                : $"The student with given id {StudentId} is not found";
        }
    }
}
