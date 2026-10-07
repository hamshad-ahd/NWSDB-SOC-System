using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NWSDB.Api.Models
{
    public class Bill
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public Customer Customer { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string BillNumber { get; set; } = string.Empty; // e.g. INV-2026-0801

        [Required]
        [StringLength(50)]
        public string BillingMonth { get; set; } = string.Empty; // e.g. August 2026

        public int UnitsUsed { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BillAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0.00m;

        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Partially Paid

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public decimal RemainingAmount => BillAmount - PaidAmount;

        // Navigation properties
        [JsonIgnore]
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [JsonIgnore]
        public ICollection<ThirdPartyCollection> ThirdPartyCollections { get; set; } = new List<ThirdPartyCollection>();
    }
}
