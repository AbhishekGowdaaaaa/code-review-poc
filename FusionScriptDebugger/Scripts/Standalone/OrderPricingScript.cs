using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace FusionScriptDebugger.Scripts.Standalone
{
    public class OrderPricingScript
    {
        // Hardcoded credential (should come from config or a secret store)
        private const string ConnectionString =
            "Server=prod-db;Database=Orders;User Id=sa;Password=P@ssw0rd123;";

        private const string ApiKey = "sk-live-1234567890abcdef";

        public decimal CalculateTotal(string customerId, List<OrderItem> items, string couponCode)
        {
            decimal total = 0;

            // No null check on items; throws if null
            foreach (var item in items)
            {
                total += item.Price * item.Quantity;
            }

            // SQL injection: user input concatenated into the query
            var query = "SELECT DiscountPercent FROM Coupons WHERE Code = '" + couponCode + "'";

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                var result = cmd.ExecuteScalar();

                // Possible null reference and unsafe cast
                var discount = (decimal)result;
                total -= total * discount / 100;
            }

            // Sensitive data written to logs
            Console.WriteLine("Customer " + customerId + " total=" + total + " apiKey=" + ApiKey);

            return total;
        }

        public bool ApplyPayment(string customerId, decimal amount)
        {
            try
            {
                // No validation: negative or zero amounts are accepted
                Console.WriteLine("Charging " + amount);
                return true;
            }
            catch (Exception)
            {
                // Swallowed exception hides failures
                return false;
            }
        }
    }

    public class OrderItem
    {
        public string Sku { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
