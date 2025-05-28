using System.Collections.Generic;

namespace SchoolAutomationApi.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public required string UserId { get; set; } // Added 'required'
        public ApplicationUser? User { get; set; }
        public string? Subject { get; set; }
        public ICollection<CourseTeacher>? CourseTeachers { get; set; }
        public ICollection<ClassSchedule>? ClassSchedules { get; set; }
    }
}