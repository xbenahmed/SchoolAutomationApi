namespace SchoolAutomationApi.Models
{
    public class ParentsStudentRelation
    {
        public int ParentId { get; set; }
        public Parent? Parent { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
    }
}