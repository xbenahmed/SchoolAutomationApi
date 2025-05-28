namespace SchoolAutomationApi.Models
{
    public class Administrator
    {
        public int Id { get; set; }
        public required string UserId { get; set; } // Added 'required'
        public ApplicationUser? User { get; set; }
    }
}