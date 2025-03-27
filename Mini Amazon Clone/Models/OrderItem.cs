using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace Mini_Amazon_Clone.Models
{
    public class OrderItem
    {
        public int OrderItemID { get; set; }

        public int OrderID { get; set; }

        public int ProductID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }  

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
