using System.Collections.Generic;

namespace SchoolAutomationApi.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public ICollection<CourseTeacher>? CourseTeachers { get; set; }
        public ICollection<StudentCourse>? StudentCourses { get; set; }
        public ICollection<ClassSchedule>? ClassSchedules { get; set; }
        public ICollection<Attendance>? Attendances { get; set; }
        public ICollection<Grade>? Grades { get; set; }
    }
}