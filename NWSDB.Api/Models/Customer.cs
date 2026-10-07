using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NWSDB.Api.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; } = string.Empty; // e.g. NWSDB-1001

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // Navigation properties
        [JsonIgnore]
        public ICollection<Bill> Bills { get; set; } = new List<Bill>();

        [JsonIgnore]
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [JsonIgnore]
        public ICollection<ThirdPartyCollection> ThirdPartyCollections { get; set; } = new List<ThirdPartyCollection>();
    }
}
