using System.Security.Cryptography;
using System.Text;
using DeliveryService.Domain.Models.Roles;
using DeliveryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Services;

public class ApiKeyService
{
    private const string Prefix = "dk_";
    private readonly DeliveryDbContext _db;
    public ApiKeyService(DeliveryDbContext db) => _db = db;

    public async Task<string> IssueAsync(long customerId)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == customerId && c.IsActive)
            ?? throw new InvalidOperationException("Заказчик не найден или заблокирован");

        var key = Prefix + Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        customer.SetApiKeyHash(Hash(key));
        await _db.SaveChangesAsync();
        return key;
    }

    public Task<Customer?> FindCustomerAsync(string key)
    {
        var hash = Hash(key.Trim());
        return _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.ApiKeyHash == hash);
    }

    private static string Hash(string key) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))).ToLowerInvariant();
}