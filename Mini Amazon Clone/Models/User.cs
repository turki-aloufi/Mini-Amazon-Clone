using System.ComponentModel.DataAnnotations;

namespace Mini_Amazon_Clone.Models
{
    public class User
    {
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }  

        [Required]
        [StringLength(50)]
        public string Role { get; set; }  

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Product> CreatedProducts { get; set; }
    }
}
