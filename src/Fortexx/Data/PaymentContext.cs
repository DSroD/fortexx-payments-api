using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;

using Fortexx.Models;

namespace Fortexx.Data {
    public class PaymentContext : DbContext, IPaymentContext {

        private IConfiguration _configuration;
        
        public PaymentContext(DbContextOptions<PaymentContext> options, IConfiguration configuration)
        : base(options) {
            _configuration = configuration;
        }

        private DbSet<Payment> Payments { get; set; }
        private DbSet<GameServer> Servers { get; set; }
        private DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Payment>()
                .HasKey(p => p.Id)
                .HasName("PK_Id");
            modelBuilder.Entity<Payment>()
                .ToTable(string.Format("{0}payments", _configuration["Database:TablePrefix"]));

            modelBuilder.Entity<GameServer>()
                .HasKey(s => s.Id)
                .HasName("PK_Id");
            modelBuilder.Entity<GameServer>()
                .ToTable(string.Format("{0}servers", _configuration["Database:TablePrefix"]));
            
            modelBuilder.Entity<Product>()
                .HasKey(pr => pr.Id)
                .HasName("Pk_Id");
            modelBuilder.Entity<Product>()
                .ToTable(string.Format("{0}products", _configuration["Database:TablePrefix"]));
            
        }

        public async Task<List<Payment>> GetLastPaymentsAsync(int number) {
            var result = await Payments
                    .OrderByDescending(p => p.Id)
                    .Take(number)
                    .Include(p => p.Product)
                    .Include(p => p.Server)
                    .ToListAsync<Payment>();
            return result;
        }

        public async Task<List<Payment>> GetNumPaymentsPageAsync(int number, int page) {
            var result = await Payments
                    .OrderByDescending(p => p.Id)
                    .Skip(number*(page-1))
                    .Take(number)
                    .Include(p => p.Product)
                    .Include(p => p.Server)
                    .ToListAsync<Payment>();
            return result;
        }

        public async Task<List<Payment>> GetPaymentsByNameAsync(string name) {
            var result = await Payments
                    .Where(p => p.User == name)
                    .OrderByDescending(p => p.Id)
                    .Include(p => p.Product)
                    .Include(p => p.Server)
                    .ToListAsync();
            return result;
        }

        public async Task<Payment> GetPaymentByIdAsync(int id) {
            var payment = await Payments
                    .Include(p => p.Product)
                    .Include(p => p.Server)
                    .FirstOrDefaultAsync(p => p.Id == id);
            return payment;
        }

        public async Task AddPaymentAsync(Payment p) {
            Payments.Add(p);
            await SaveChangesAsync();
        }

        public async Task<ActivatePaymentResult> ActivatePaymentAsync(int id) {
            var payment = await Payments
                    .FindAsync(id);
            if(payment == null) {
                return new ActivatePaymentResult() {
                    Result = null,
                    Response = ActivatePaymentResponse.NOT_FOUND
                };
            }
            if(payment.Activated) {
                return new ActivatePaymentResult() {
                    Result = payment,
                    Response = ActivatePaymentResponse.ALREADY_ACTIVE
                };
            }
            payment.Activated = true;
            await SaveChangesAsync();
            return new ActivatePaymentResult() {
                    Result = payment,
                    Response = ActivatePaymentResponse.ACTIVATED
                };
        }

        public async Task AddGameServerAsync(GameServer s) {
            Servers.Add(s);
            await SaveChangesAsync();
        }

        public async Task<GetGameServerResult> GetGameServerByNameAsync(string name) {
            var server = await Servers
                    .FirstOrDefaultAsync(s => s.Name == name);
            if (server == null) {
                return new GetGameServerResult {
                    Result = null,
                    Response = GetObjectResponse.NOT_FOUND
                };
            }
            return new GetGameServerResult {
                    Result = server,
                    Response = GetObjectResponse.FOUND
                };
        }

        public async Task<GetGameServerResult> GetGameServerByIdAsync(int id) {
             var server = await Servers
                    .FindAsync(id);
            if (server == null) {
                return new GetGameServerResult {
                    Result = null,
                    Response = GetObjectResponse.NOT_FOUND
                };
            }
            return new GetGameServerResult {
                    Result = server,
                    Response = GetObjectResponse.FOUND
                };
        }

        public async Task<List<GameServer>> GetGameServersAsync() {
            var servers = await Servers
                    .OrderBy(s => s.Id)
                    .Take(1024)
                    .ToListAsync();
            return servers;
        }

        public async Task AddProductAsync(Product p) {
            Products.Add(p);
            await SaveChangesAsync();
        }

        public async Task<GetProductResult> GetProductByIdAsync(int id) {
            var result = await Products
                    .Include(p => p.GameServer)
                    .FirstOrDefaultAsync(p => p.Id == id);
            if (result == null) {
                return new GetProductResult {
                    Result = null,
                    Response = GetObjectResponse.NOT_FOUND
                };
            }
            return new GetProductResult {
                Result = result,
                Response = GetObjectResponse.FOUND
            };
        }

        public async Task<List<Product>> GetServerProductsAsync(int serverId) {
            var server = await Servers
                    .Include(s => s.Products)
                    .FirstOrDefaultAsync(s => s.Id == serverId);
            var products = server?.Products ?? new List<Product>();
            return products;
        }

    }
}