using ContactsSync.Repository;
using ContactsSync.Integration;
using ContactsSync.Dtos;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseQueryTrackingBehavior(Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking));
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddTransient<IMailchimp, Mailchimp>();
builder.Services.AddHttpClient();
builder.Services.AddCors();
builder.Services.AddSwaggerGen(opts => opts.EnableAnnotations());
var app = builder.Build();

app.UseSwagger();
app.MapSwagger();
app.UseSwaggerUI();
app.UseCors(p => p.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod());
app.UseHttpsRedirection();

app.MapGet("/contacts/sync",
[SwaggerOperation(Summary = "Syncs contacts", Description = "Syncs contacts between mockAPI and mailChimp")]
[SwaggerResponse(200, "Syncs successfully", typeof(ContactDetailDto))]
[SwaggerResponse(500, "Internal error with internal database or mailChimp integration")]
 async (IContactRepository contactRepo, IHttpClientFactory factory, IMailchimp mailChimp) => 
{
    List<ContactDetailDto> currentContacts = new List<ContactDetailDto>();
    List<ContactDetailDto> filteredContacts = new List<ContactDetailDto>();
    List<ContactDetailDto> newContacts = new List<ContactDetailDto>();

    // Get contacts from mockApi
    var httpClient = factory.CreateClient();
    var response = await httpClient.GetFromJsonAsync<List<ContactDetailDto>>("https://613b9035110e000017a456b1.mockapi.io/api/v1/contacts");
    
    if(response != null)
    {
        currentContacts = await contactRepo.GetAllDetail();

        // Filter local contacts to prevent duplicate syncs
        if(currentContacts.Count > 0)
            filteredContacts = response.Except(currentContacts).ToList();
        else
            filteredContacts = response;
        
        // Syncs Mailchimp with local data
        foreach(ContactDetailDto dto in filteredContacts)
        {
            // MailChimp Integration - Add member
            ContactDetailDto contactDto = await mailChimp.AddMember(dto);
            
            // If add MailChimp member success, add local data to prevent duplicate syncs in future
            if(!string.IsNullOrEmpty(contactDto.MailchimpMemberId))
            {
                var newCreatedContact = await contactRepo.Add(contactDto);
                
                if(newCreatedContact == null)
                    return Results.Problem($"Fail inserting contact with ID {contactDto.Id} into local database.", statusCode: 500);

                newContacts.Add(contactDto);
            }
            else
                return Results.Problem($"Fail on Mailchimp integration for {contactDto.Email}.", statusCode: 500);
        }
    }
    
    return Results.Ok(newContacts);

}).ProducesProblem(500).Produces<List<ContactDetailDto>>(StatusCodes.Status200OK).WithDisplayName("Syncs contacts between mockAPI and mailChimp");

app.MapGet("/contacts", 
[SwaggerOperation(Summary = "Get all contacts", Description = "Get all local contacts")]
[SwaggerResponse(200, "Get all contacts successfully", typeof(ContactDto))]
(IContactRepository contactRepo) => 
{
    return contactRepo.GetAll();
}).Produces<ContactDto[]>(StatusCodes.Status200OK).WithName("Get all local contacts");

app.MapGet("/contact/{contactId:int}", 
[SwaggerOperation(Summary = "Syncs contact by id", Description = "Get a local contact by Id")]
[SwaggerResponse(200, "Get local contact by id successfully", typeof(ContactDetailDto))]
[SwaggerResponse(404, "Contact not found")]
async (int contactId, IContactRepository contactRepo) => 
{
    var contact = await contactRepo.GetById(contactId);

    if(contact == null)
        return Results.Problem($"Contact with ID {contactId} not found.", statusCode: 404);
    
    return Results.Ok(contact);

}).ProducesProblem(404).Produces<ContactDetailDto>(StatusCodes.Status200OK).WithName("Get a local contact by Id");

app.MapPost("/contacts", 
[SwaggerOperation(Summary = "Create a new contact", Description = "Create a new single local and mailchimp contact")]
[SwaggerResponse(201, "Contact created successfully ", typeof(ContactDetailDto))]
[SwaggerResponse(400, "Bad Request or Contact already exists")]
[SwaggerResponse(500, "Internal error with internal database or mailChimp integration")]
async ([FromBody] ContactDetailDto dto, IContactRepository contactRepo, IMailchimp mailChimp) => 
{
    if(dto == null)
        return Results.Problem($"A contact needs to be informed.", statusCode: 400);
    
    // Check if contact exists in local database
    ContactDetailDto contact = await contactRepo.GetById(dto.Id);

    if(contact != null)
        return Results.Problem($"Contact with ID {dto.Id} already exists.", statusCode: 400);

    // Add mailChimpmember
    ContactDetailDto contactDto = await mailChimp.AddMember(dto);
            
    // If add MailChimp member success, add local data to prevent duplicate syncs in future
    if(!string.IsNullOrEmpty(contactDto.MailchimpMemberId))
    {
        var newCreatedContact = await contactRepo.Add(contactDto);
        
        if(newCreatedContact == null)
            return Results.Problem($"Fail inserting contact with ID {contactDto.Id} into local database.", statusCode: 500);
    }
    else
        return Results.Problem($"Fail on Mailchimp integration for {contactDto.Email}.", statusCode: 500);

return Results.Created($"/contacts/{contactDto.Id}", contactDto);
}).Produces<ContactDetailDto>(StatusCodes.Status201Created).WithName("Create a new single local and mailchimp contact");

app.MapDelete("/contacts/{contactId:int}", 
[SwaggerOperation(Summary = "Delete a contact", Description = "Delete a local contact and mailChimp contact")]
[SwaggerResponse(200, "Contact deleted successfully")]
[SwaggerResponse(404, "Contact not found")]
[SwaggerResponse(500, "Internal error with internal database or mailChimp integration")]
async (int contactId, IContactRepository contactRepo, IMailchimp mailChimp) =>
{
    // Check if contact exists in local database
    ContactDetailDto contact = await contactRepo.GetById(contactId);

    if(contact == null)
        return Results.Problem($"Contact with ID {contactId} not found.", statusCode: 404);
    
    // If exists in local, remove from mailchimp providing memberId
    bool resultMailchimpRemove = await mailChimp.RemoveMember(contact.MailchimpMemberId);
    
    // If remove from mailchimp was success, remove in local database too
    if(resultMailchimpRemove)
        await contactRepo.Delete(contactId);
    else
        return Results.Problem($"Fail deleting contact with ID {contactId} with mailchimp memberid {contact.MailchimpMemberId}.", statusCode: 500);

    return Results.Ok();

}).ProducesProblem(404).Produces(StatusCodes.Status200OK).WithName("Delete a local contact and mailChimp contact");

app.Run();