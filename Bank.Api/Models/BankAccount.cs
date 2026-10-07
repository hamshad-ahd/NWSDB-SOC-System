using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Bank.Api.Models
{
    public class BankAccount
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CardHolderName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string ExpiryDate { get; set; } = string.Empty; // MM/YY

        [Required]
        [StringLength(10)]
        public string CVV { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
    }
}
