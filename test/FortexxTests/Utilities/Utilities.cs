using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Fortexx.Data;
using Fortexx.Models;
using Fortexx.Models.Api;

namespace FortexxTests {


    public static class Utilities {

        private static readonly Random random = new Random(); 
        private static readonly object syncLock = new object(); 

        public static int RandomNumber(int min, int max) {
            lock(syncLock) { // synchronize
                return random.Next(min, max);
            }
        }
        public static DbContextOptions<PaymentContext> TestDbContextOptions() {
            
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<PaymentContext>()
                .UseInMemoryDatabase("MemoryDb")
                .UseInternalServiceProvider(serviceProvider);

            return builder.Options;
        }

        public static IConfiguration BuildTestConfiguration() {
            var inMemorySettings = new Dictionary<string, string> {
                {"Database:TablePrefix", "prefix_"},
                {"Route:LimitedKey", "limitedKey"},
                {"Route:Key", "key"},
                {"Route:SuperUserKey", "superUserKey"},
            };
            return new ConfigurationBuilder()
                        .AddInMemoryCollection(inMemorySettings)
                        .Build();
        }

        public static Payment GenerateRandomPayment(int id) {
            return GenerateRandomPayment(id, "test");
        }

        public static Payment GenerateRandomPayment(int id, string username) {
            var p = new Payment {
                    Id = id,
                    PaymentId = 101,
                    PaymentDate = DateTime.Now,
                    PaymentType = "Paymen tType",
                    Value = RandomNumber(1, 9999),
                    Currency = "CURRENCY",
                    User = username,
                    MainInfo = "moreInfo",
                    OtherInfo = "other",
                    Status = "WAITING",
                    Activated = false
                };
            return p;
        }

        public static Payment GenerateRandomPaymentWithServerProduct(int id, string username, int? serverId, int? productId) {
            var p = new Payment {
                    Id = id,
                    PaymentId = 101,
                    PaymentDate = DateTime.Now,
                    PaymentType = "Paymen tType",
                    Value = RandomNumber(1, 9999),
                    Currency = "CURRENCY",
                    User = username,
                    ServerId = serverId,
                    ProductId = productId,
                    MainInfo = "moreInfo",
                    OtherInfo = "other",
                    Status = "WAITING",
                    Activated = false
                };
            return p;
        }

        public static PaymentDto GenerateRandomPaymentDto(int id) {
            var p = GenerateRandomPaymentDto(id, "test");
            return p;
        }

        public static PaymentDto GenerateRandomPaymentDto(int id, string username) {
            var p = new PaymentDto {
                    Id = id,
                    PaymentId = 101,
                    PaymentDate = DateTime.Now,
                    PaymentType = "Paymen tType",
                    Value = RandomNumber(1, 9999),
                    Currency = "CURRENCY",
                    User = username,
                    ServerId = 60,
                    ServerName = "server_name",
                    ProductId = 70,
                    ProductName = "product",
                    MainInfo = "moreInfo",
                    OtherInfo = "other",
                    Status = "WAITING",
                    Activated = false
                };
            return p;
        }

        public static LimitedPaymentDto ToLimitedDto(PaymentDto p) {
            var lp = new LimitedPaymentDto {
                Id = p.Id,
                PaymentDate = p.PaymentDate,
                PaymentType = p.PaymentType,
                Value = p.Value,
                Currency = p.Currency,
                User = p.User,
                ServerId = p.ServerId,
                ServerName = p.ServerName,
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Activated = p.Activated
            };
            return lp;
        }


    }

}