namespace ContactsSync.Dtos
{
    public record ContactDetailDto(int Id, string? FirstName, string? LastName, string? Email, string? Avatar, DateTime? CreatedAt, string? MailchimpMemberId);  
}
