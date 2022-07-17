using Microsoft.AspNetCore.Identity;

namespace BookStoreAPI.Data.Entities
{
    public class User : IdentityUser
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
    }
}
