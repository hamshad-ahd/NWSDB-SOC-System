using Microsoft.AspNetCore.Mvc;
using ThirdParty.WebApp.Models.ViewModels;
using ThirdParty.WebApp.Services;

namespace ThirdParty.WebApp.Controllers
{
    public class CollectionController : Controller
    {
        private readonly INwsdbApiClient _apiClient;

        public CollectionController(INwsdbApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public IActionResult Lookup()
        {
            return View(new CustomerLookupViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Lookup(CustomerLookupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var searchTerm = (model.SearchTerm ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                model.ErrorMessage = "Please enter an Account Number.";
                return View(model);
            }

            var customer = await _apiClient.SearchCustomerAsync(searchTerm);
            if (customer == null)
            {
                model.ErrorMessage = $"NWSDB Customer account '{searchTerm}' not found.";
                return View(model);
            }

            return RedirectToAction("BillDetails", new { accountNumber = customer.AccountNumber });
        }

        [HttpGet]
        public async Task<IActionResult> BillDetails(string accountNumber)
        {
            var acc = (accountNumber ?? string.Empty).Trim();
            var customer = await _apiClient.SearchCustomerAsync(acc);
            if (customer == null) return NotFound("Customer not found.");

            var activeBill = await _apiClient.GetActiveBillAsync(acc);
            var history = await _apiClient.GetBillHistoryAsync(acc);

            if (activeBill != null)
            {
                activeBill.Customer = customer;
            }

            foreach (var b in history)
            {
                b.Customer = customer;
            }

            var vm = new CustomerBillDetailsViewModel
            {
                Customer = customer,
                ActiveBill = activeBill,
                AllBills = history
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Collect(string billNumber)
        {
            var billNum = (billNumber ?? string.Empty).Trim();
            var bill = await _apiClient.GetBillAsync(billNum);
            if (bill == null) return NotFound("Bill not found.");

            if (bill.DueAmount <= 0)
            {
                TempData["ErrorMessage"] = $"Bill '{billNum}' is already fully paid with zero outstanding balance.";
                return RedirectToAction("BillDetails", new { accountNumber = bill.Customer?.AccountNumber });
            }

            var model = new PaymentCollectionViewModel
            {
                BillNumber = bill.BillNumber,
                AccountNumber = bill.Customer?.AccountNumber ?? "",
                CustomerName = bill.Customer?.Name ?? "",
                AmountDue = bill.DueAmount,
                AmountToCollect = bill.DueAmount,
                PaymentMethod = "Cash",
                CollectorId = "AGENT-5501",
                CashTendered = bill.DueAmount,
                CardHolderName = bill.Customer?.Name ?? string.Empty,
                CardNumber = string.Empty,
                CardExpiry = string.Empty,
                CardCvc = string.Empty
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Collect(PaymentCollectionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ErrorMessage = string.Join(" ",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                return View(model);
            }

            if (model.AmountToCollect <= 0)
            {
                model.ErrorMessage = "Collection amount must be greater than zero.";
                return View(model);
            }

            if (model.AmountDue > 0 && model.AmountToCollect > model.AmountDue)
            {
                model.ErrorMessage = $"Collection amount (Rs. {model.AmountToCollect:N2}) cannot exceed the amount due (Rs. {model.AmountDue:N2}).";
                return View(model);
            }

            string? bankTxId = null;
            int collectionId = 0;

            // STAGE 1 — COLLECTION
            // If CASH: money is collected by supermarket/agent; recorded as Collected in NWSDB with TransferStatus = Pending.
            // Bank.Api is NEVER called for Cash collections.
            if (model.PaymentMethod == "Cash")
            {
                if (model.CashTendered < model.AmountToCollect)
                {
                    model.ErrorMessage = "Cash tendered is less than the collection amount.";
                    return View(model);
                }

                var cashRes = await _apiClient.CollectCashPaymentAsync(model);
                if (!cashRes.Success)
                {
                    model.ErrorMessage = $"Cash collection failed: {cashRes.Message}";
                    return View(model);
                }

                collectionId = cashRes.CollectionId;
            }
            // If CARD: Authorize & process card via Bank.Api BEFORE enabling Transfer
            else if (model.PaymentMethod == "Card")
            {
                if (string.IsNullOrWhiteSpace(model.CardNumber))
                {
                    model.ErrorMessage = "Please enter a valid card number.";
                    return View(model);
                }
                if (string.IsNullOrWhiteSpace(model.CardExpiry))
                {
                    model.ErrorMessage = "Please enter card expiry date (MM/YY).";
                    return View(model);
                }
                if (string.IsNullOrWhiteSpace(model.CardCvc))
                {
                    model.ErrorMessage = "Please enter card CVV/CVC code.";
                    return View(model);
                }

                var cardRes = await _apiClient.CollectCardPaymentAsync(model);
                if (!cardRes.Success)
                {
                    model.ErrorMessage = $"Card collection failed: {cardRes.Message}";
                    return View(model);
                }

                bankTxId = cardRes.BankTransactionId;
                collectionId = cardRes.CollectionId;
            }

            // Record Collection as Collected (CollectionStatus = Collected, TransferStatus = Pending)
            var transferModel = new TransferViewModel
            {
                CollectionId = collectionId,
                BillNumber = model.BillNumber,
                AccountNumber = model.AccountNumber,
                CustomerName = model.CustomerName,
                CollectedAmount = model.AmountToCollect,
                PaymentMethod = model.PaymentMethod,
                CollectorId = model.CollectorId,
                CollectionTimestamp = DateTime.UtcNow,
                BankTransactionId = bankTxId,
                CollectionStatus = "Collected",
                TransferStatus = "Pending"
            };

            // Store in isolated, collection-specific TempData
            var key = $"Transfer_{collectionId}";
            TempData[key] = System.Text.Json.JsonSerializer.Serialize(transferModel);
            return RedirectToAction("Transfer", new { collectionId = collectionId });
        }

        [HttpGet]
        public IActionResult Transfer(int? collectionId)
        {
            if (!collectionId.HasValue || collectionId.Value <= 0)
            {
                return RedirectToAction("Lookup");
            }

            var key = $"Transfer_{collectionId.Value}";
            var json = TempData[key] as string;
            if (string.IsNullOrEmpty(json))
            {
                return View(new TransferViewModel { CollectionId = collectionId.Value });
            }

            TempData.Keep(key);
            var model = System.Text.Json.JsonSerializer.Deserialize<TransferViewModel>(json);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TransferSubmit(TransferViewModel formModel)
        {
            if (formModel.CollectionId <= 0)
            {
                return RedirectToAction("Lookup");
            }

            // STAGE 2 — TRANSFER / SETTLEMENT
            // Execute Transfer to NWSDB (Settlement Action - Identifies existing collection via CollectionId, Does NOT call Bank.Api)
            var result = await _apiClient.TransferCollectionAsync(formModel);

            if (!result.Success)
            {
                formModel.ErrorMessage = result.Message;
                var key = $"Transfer_{formModel.CollectionId}";
                TempData[key] = System.Text.Json.JsonSerializer.Serialize(formModel);
                return View("Transfer", formModel);
            }

            var receiptKey = $"Receipt_{result.ReceiptNumber}";
            TempData[receiptKey] = System.Text.Json.JsonSerializer.Serialize(result);
            return RedirectToAction("Receipt", new { receiptNumber = result.ReceiptNumber });
        }

        [HttpGet]
        public IActionResult Receipt(string? receiptNumber)
        {
            if (string.IsNullOrEmpty(receiptNumber))
            {
                return RedirectToAction("Lookup");
            }

            var receiptKey = $"Receipt_{receiptNumber}";
            var json = TempData[receiptKey] as string;
            if (string.IsNullOrEmpty(json))
            {
                return RedirectToAction("Lookup");
            }

            var receipt = System.Text.Json.JsonSerializer.Deserialize<SettlementReceiptViewModel>(json);
            TempData.Keep(receiptKey);
            return View(receipt);
        }
    }
}
