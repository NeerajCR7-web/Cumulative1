using Cumulative1.Models;

namespace Cumulative1.Models
{
    public class TeacherSearchModel
    {
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public required List<Teacher> Teachers { get; set; }
    }
}
