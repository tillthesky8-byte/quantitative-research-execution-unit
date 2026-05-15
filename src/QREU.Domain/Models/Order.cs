
namespace Domain.Models;

public sealed class Order(OrderRequest request, long timestamp)
{
    public Guid Id { get; } = Guid.NewGuid();
    public long Timestamp = timestamp;
    public OrderRequest Request = request;
}