using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public class AuthorForUpdate
    {  
        [Required]
        [MaxLength(150)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(150)]
        public string LastName { get; set; }
    }
}
