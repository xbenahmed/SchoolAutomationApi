using System;

namespace SchoolAutomationApi.Models
{
    public class ClassSchedule
    {
        public int ScheduleId { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public int TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        public DateTime ClassTime { get; set; }
        public string? RoomNumber { get; set; }
    }
}