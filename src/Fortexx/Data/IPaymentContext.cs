using System.Collections.Generic;
using System.Threading.Tasks;

using Fortexx.Models;
using Fortexx.Models.Api;

namespace Fortexx.Data {

    public interface IPaymentContext {

        public Task<List<Payment>> GetLastPaymentsAsync(int number);

        public Task<List<Payment>> GetNumPaymentsPageAsync(int number, int page);

        public Task<List<Payment>> GetPaymentsByNameAsync(string name);

        public Task<Payment> GetPaymentByIdAsync(int id);

        public Task AddPaymentAsync(Payment p);

        public Task<ActivatePaymentResult> ActivatePaymentAsync(int id);

        public Task AddGameServerAsync(GameServer s);

        public Task UpdateGameServerAsync(GameServerDto s);

        public Task<GetGameServerResult> GetGameServerByNameAsync(string name);

        public Task<GetGameServerResult> GetGameServerByIdAsync(int id);

        public Task<List<GameServer>> GetGameServersAsync();

        public Task AddProductAsync(Product p);
        
        public Task<GetProductResult> GetProductByIdAsync(int id);

        public Task<List<Product>> GetServerProductsAsync(int serverId);

    }

    public enum ActivatePaymentResponse {
        ACTIVATED,
        ALREADY_ACTIVE,
        NOT_FOUND
    }

    public record ActivatePaymentResult {

        public Payment? Result { get; init; }
        public ActivatePaymentResponse Response { get; init; }
    }

    public enum GetObjectResponse {
        FOUND,
        NOT_FOUND
    }

    public record GetGameServerResult {
        public GameServer? Result { get; init; }
        public GetObjectResponse Response { get; init; }
    }

    public record GetProductResult {
        public Product? Result { get; init; }
        public GetObjectResponse Response { get; init; }
    }

    public record GetProductsResult {
        public List<Product>? Result { get; init; }
        public GetObjectResponse Response { get; init; }
    }

}