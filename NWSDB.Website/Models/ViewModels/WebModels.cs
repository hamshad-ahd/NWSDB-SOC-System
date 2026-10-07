using System.ComponentModel.DataAnnotations;

namespace NWSDB.Website.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Account Number is required.")]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; } = "NWSDB-1001";

        [Required(ErrorMessage = "Customer Name is required.")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = "Kavindu Perera";

        public string? ErrorMessage { get; set; }
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class BillDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public CustomerDto? Customer { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public string BillingMonth { get; set; } = string.Empty;
        public int UnitsUsed { get; set; }
        public decimal BillAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public decimal DueAmount => BillAmount - PaidAmount;
    }

    public class PaymentRecordDto
    {
        public int Id { get; set; }
        public int BillId { get; set; }
        public int CustomerId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CustomerDashboardDto
    {
        public CustomerDto Customer { get; set; } = null!;
        public BillDto? ActiveBill { get; set; }
        public List<PaymentRecordDto> PaymentHistory { get; set; } = new();
    }

    public class CardPaymentViewModel
    {
        public string BillNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal AmountToPay { get; set; }

        [Required(ErrorMessage = "Card Number is required.")]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; } = "4532718293841029";

        [Required(ErrorMessage = "Expiry Date is required.")]
        [Display(Name = "Expiry (MM/YY)")]
        public string CardExpiry { get; set; } = "12/28";

        [Required(ErrorMessage = "CVC Security Code is required.")]
        [Display(Name = "CVC Code")]
        public string CardCvc { get; set; } = "123";

        [Required(ErrorMessage = "Cardholder Name is required.")]
        [Display(Name = "Cardholder Name")]
        public string CardHolderName { get; set; } = "Kavindu Perera";

        public string? ErrorMessage { get; set; }
    }

    public class PaymentReceiptViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ReceiptNumber { get; set; } = string.Empty;
        public string BillNumber { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = "Card";
        public string Channel { get; set; } = "NWSDB Website";
        public string? BankTransactionId { get; set; }
        public DateTime PaidAt { get; set; }
        public decimal RemainingBillBalance { get; set; }
        public string BillStatus { get; set; } = string.Empty;
    }

    public class WaterUsageDto
    {
        public string MonthYear { get; set; } = string.Empty;
        public int UnitsConsumed { get; set; }
        public DateTime ReadingDate { get; set; }
    }

    public class LoginApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccountNumber { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
