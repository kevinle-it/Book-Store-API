using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.Annotations;

namespace BookStoreAPI.Data.Entities
{
    [SwaggerSchema(ReadOnly = true)]
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }

        public Cart Cart { get; set; }
        public ICollection<Order> Order { get; set; }
    }
}
