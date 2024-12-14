using Cumulative1.Models;

namespace SchoolDatabase.Models
{
    public class TeacherSearchModel
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public List<Teacher> Teachers { get; set; }
    }
}
