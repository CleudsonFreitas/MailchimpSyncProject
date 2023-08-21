namespace ContactsSync.Dtos
{
    public record MergeFields(string? FNAME, string? LNAME);
    public record MailchimpMemberDto(string? email_address, string? full_name, MergeFields merge_fields, string? status = "subscribed");  
}
