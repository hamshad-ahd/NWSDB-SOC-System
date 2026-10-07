using Bank.Api.Models;

namespace Bank.Api.Data
{
    public static class BankDbInitializer
    {
        public static void Initialize(BankDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.BankAccounts.Any())
            {
                return; // DB already seeded
            }

            var accounts = new BankAccount[]
            {
                // Card 1: Valid active card with high balance
                new BankAccount
                {
                    CardNumber = "4532718293841029",
                    CardHolderName = "Kavindu Perera",
                    ExpiryDate = "12/28",
                    CVV = "123",
                    Balance = 75000.00m,
                    IsActive = true
                },
                // Card 2: Valid active card
                new BankAccount
                {
                    CardNumber = "5412751234567890",
                    CardHolderName = "Nimali Fernando",
                    ExpiryDate = "09/27",
                    CVV = "456",
                    Balance = 32500.50m,
                    IsActive = true
                },
                // Card 3: Valid active card with low balance (Insufficient balance test)
                new BankAccount
                {
                    CardNumber = "4000123456789010",
                    CardHolderName = "Saman Silva",
                    ExpiryDate = "11/26",
                    CVV = "789",
                    Balance = 1500.00m,
                    IsActive = true
                },
                // Card 4: Expired card (Expired card test)
                new BankAccount
                {
                    CardNumber = "4111111111111111",
                    CardHolderName = "Dilani Jayawardena",
                    ExpiryDate = "12/22",
                    CVV = "111",
                    Balance = 50000.00m,
                    IsActive = true
                },
                // Card 5: Inactive card (Inactive card test)
                new BankAccount
                {
                    CardNumber = "4222222222222222",
                    CardHolderName = "Ruwan Gunasekara",
                    ExpiryDate = "12/29",
                    CVV = "222",
                    Balance = 50000.00m,
                    IsActive = true
                }
            };

            context.BankAccounts.AddRange(accounts);
            context.SaveChanges();
        }

        public static void ResetTestData(BankDbContext context)
        {
            var initialAccounts = new Dictionary<string, (decimal balance, bool active, string expiry)>
            {
                { "4532718293841029", (75000.00m, true, "12/28") },
                { "5412751234567890", (32500.50m, true, "12/27") },
                { "4000123456789010", (1500.00m, true, "11/26") },
                { "4111111111111111", (50000.00m, true, "12/22") },
                { "4222222222222222", (50000.00m, true, "12/29") }
            };

            foreach (var account in context.BankAccounts)
            {
                if (initialAccounts.TryGetValue(account.CardNumber, out var config))
                {
                    account.Balance = config.balance;
                    account.IsActive = config.active;
                    account.ExpiryDate = config.expiry;
                }
            }

            context.SaveChanges();
        }
    }
}
