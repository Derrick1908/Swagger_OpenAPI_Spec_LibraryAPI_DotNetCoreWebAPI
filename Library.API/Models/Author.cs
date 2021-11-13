using System;

namespace Library.API.Models
{
    /// <summary>
    /// An Author with Id, FirstName and LastName Fields
    /// </summary>
    public class Author
    {        
        /// <summary>
        /// The Id of the Author
        /// </summary>
        public Guid Id { get; set; } 
     
        /// <summary>
        /// The First Name of the Author
        /// </summary>
        public string FirstName { get; set; }
      
        /// <summary>
        /// The Last Name of the Author
        /// </summary>
        public string LastName { get; set; }
    }
}
