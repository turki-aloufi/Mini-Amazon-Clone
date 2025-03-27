using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mini_Amazon_Clone.Models
{
    public class Product
    {
        public int ProductID { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public int CreatedBy { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual User Creator { get; set; }
        [JsonIgnore]
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
