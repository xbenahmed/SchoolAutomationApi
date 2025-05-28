using System;

namespace SchoolAutomationApi.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public DateTime Date { get; set; }
        public string? Status { get; set; }
    }
}