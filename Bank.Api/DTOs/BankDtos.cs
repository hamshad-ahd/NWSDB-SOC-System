using System.ComponentModel.DataAnnotations;

namespace Bank.Api.DTOs
{
    public class BankPaymentRequestDto
    {
        [Required(ErrorMessage = "Card number is required.")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cardholder name is required.")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry date is required.")]
        public string ExpiryDate { get; set; } = string.Empty; // MM/YY

        [Required(ErrorMessage = "CVV is required.")]
        public string CVV { get; set; } = string.Empty;

        [Range(0.01, 1000000.00, ErrorMessage = "Payment amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }

    public class BankPaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal RemainingBalance { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
