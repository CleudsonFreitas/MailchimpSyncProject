namespace ContactsSync.Repository.Entities
{
    public class ContactEntity
    {
        public int Id { get; set; }
        public string? Avatar { get; set; }
        public string? Email { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? MailchimpMemberId { get; set; }
    }
}
