using ContactsSync.Dtos;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ContactsSync.Integration
{
    public interface IMailchimp
    {
        Task<ContactDetailDto> AddMember(ContactDetailDto dto);
        Task<bool> RemoveMember(string memberId);
    }

    public class AddMemberResponse
    {
        public string id { get; set; }
        public string email_address { get; set; }
    }

    public class Mailchimp : IMailchimp
    {
        string AudienceID = "cc55800fc9";
        IHttpClientFactory HttpFactory;

        public Mailchimp(IHttpClientFactory factory)
        {
          HttpFactory = factory;  
        }
        
        public async Task<ContactDetailDto> AddMember(ContactDetailDto dto)
        {
            try
            {
                    MailchimpMemberDto member = new MailchimpMemberDto(
                    dto.Email, 
                    $"{dto.FirstName} {dto.LastName}",
                    new MergeFields(dto.FirstName, dto.LastName),
                    "subscribed");

                string jsonString = JsonSerializer.Serialize(member);

                var httpClient = HttpFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "e5dd6580abef369bcd4b6e9acbd5836a-us21");
                var response = await httpClient.PostAsync($"https://us21.api.mailchimp.com/3.0/lists/{AudienceID}/members", new StringContent(jsonString));
                response.EnsureSuccessStatusCode();
                AddMemberResponse body = response.Content.ReadFromJsonAsync<AddMemberResponse>().GetAwaiter().GetResult();
                
                if(!string.IsNullOrEmpty(body.id))
                    return dto with {MailchimpMemberId = body.id};
            }
            catch(Exception ex)
            {
                //log internal exception ex
                return dto;
            }

            return dto;
        }
    
        public async Task<bool> RemoveMember(string memberId)
        {
            try
            {
                var httpClient = HttpFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "e5dd6580abef369bcd4b6e9acbd5836a-us21");
                var response = await httpClient.DeleteAsync($"https://us21.api.mailchimp.com/3.0/lists/{AudienceID}/members/{memberId}");
                response.EnsureSuccessStatusCode();
                return response.IsSuccessStatusCode;
            }
            catch(Exception ex)
            {
                // Log internal exception ex
                return false;
            }            
        }
    }
}