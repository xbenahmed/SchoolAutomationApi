namespace SchoolAutomationApi.Models
{
    public class Parent
    {
        public int Id { get; set; }
        public required string UserId { get; set; } // Added 'required'
        public ApplicationUser? User { get; set; }
        public ICollection<ParentsStudentRelation>? ParentStudentRelations { get; set; }
    }
}