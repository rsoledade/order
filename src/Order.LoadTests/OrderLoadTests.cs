using System.Net.Http;
using System.Text.Json;
using System.Text;
using Xunit;
using System;

namespace Order.Tests.Domain
{
    public class OrderLoadTests
    {
        private const string BaseUrl = "http://localhost:5000";
        private const string Endpoint = "/api/orders/register-order";

        private static object GenerateOrder()
        {
            return new
            {
                externalId = Guid.NewGuid().ToString(),
                products = new[]
                {
                        new {
                            name = $"Product {Random.Shared.Next(1, 1000)}",
                            price = Random.Shared.Next(1, 100),
                            quantity = Random.Shared.Next(1, 5)
                        }
                    }
            };
        }

        // This is a placeholder for a load test. No unused usings or code remain.
    }
}
