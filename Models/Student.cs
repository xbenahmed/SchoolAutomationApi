using System.Collections.Generic;

namespace SchoolAutomationApi.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public required string UserId { get; set; } // Already string, confirmed
        public ApplicationUser? User { get; set; }
        public string? QRCode { get; set; }
        public int? ClassId { get; set; }
        public Class? Class { get; set; }
        public string? Name { get; set; }
        public string? GradeLevel { get; set; }
        public string? ContactInfo { get; set; }
        public ICollection<ParentsStudentRelation>? ParentStudentRelations { get; set; }
        public ICollection<StudentCourse>? StudentCourses { get; set; }
        public ICollection<Attendance>? Attendances { get; set; }
        public ICollection<Grade>? Grades { get; set; }
    }
}