using Microsoft.EntityFrameworkCore;
using ContactsSync.Repository.Entities;
using ContactsSync.Dtos;

namespace ContactsSync.Repository
{
    public interface IContactRepository
    {
        Task<List<ContactDto>> GetAll();
        Task<List<ContactDetailDto>> GetAllDetail();
        Task<ContactDetailDto?> GetById(int id);
        Task<ContactDetailDto?> GetLast();
        Task<ContactDetailDto> Add(ContactDetailDto dto);
        Task Delete(int id);
    }

    public class ContactRepository : IContactRepository
    {
        private readonly ApplicationDbContext _context;

        public ContactRepository(ApplicationDbContext context)
        {
            this._context = context;
        }

        public async Task<List<ContactDto>> GetAll()
        {
            return await _context.Contacts.Select(c => new ContactDto(c.Id, c.FirstName, c.LastName, c.Email)).ToListAsync();
        }

        public async Task<List<ContactDetailDto>> GetAllDetail()
        {
            return await _context.Contacts.Select(c => new ContactDetailDto(c.Id, c.FirstName, c.LastName, c.Email, c.Avatar, c.CreatedAt, null)).ToListAsync();
        }

        public async Task<ContactDetailDto?> GetById(int id)
        {
            var e = await _context.Contacts.SingleOrDefaultAsync(c => c.Id == id);

            if(e == null)
                return null;
            
            return EntityToContactDetailDto(e);
        }

        public async Task<ContactDetailDto?> GetLast()
        {
            var e = await _context.Contacts.OrderBy(c => c.Id).LastOrDefaultAsync();

            if(e == null)
                return null;
            
            return EntityToContactDetailDto(e);
        }

        public async Task<ContactDetailDto> Add(ContactDetailDto dto)
        {
            var entity = new ContactEntity();
            DetailDtoToContactEntity(dto, entity);

            _context.Contacts.Add(entity);
            await _context.SaveChangesAsync();

            return EntityToContactDetailDto(entity);
        }

        public async Task Delete(int id)
        {
            var entity = await _context.Contacts.FindAsync(id);

            if(entity == null)
                throw new ArgumentException($"Error deleting contact {id}");
            
            _context.Contacts.Remove(entity);
            await _context.SaveChangesAsync();
        }

        private void DetailDtoToContactEntity(ContactDetailDto dto, ContactEntity entity)
        {
            entity.Id = dto.Id;
            entity.Email = dto.Email;
            entity.FirstName = dto.FirstName;
            entity.LastName = dto.LastName;
            entity.Avatar = dto.Avatar;
            entity.CreatedAt = dto.CreatedAt;
            entity.MailchimpMemberId = dto.MailchimpMemberId;
        }

        private ContactDetailDto EntityToContactDetailDto(ContactEntity entity)
        {
            return new ContactDetailDto(entity.Id, entity.FirstName, entity.LastName, entity.Email, entity.Avatar, entity.CreatedAt, entity.MailchimpMemberId);
        }
    }
}