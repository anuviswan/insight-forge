using Azure;
using Azure.Data.Tables;

namespace Insight.Services.Core.Domain.Entities;

public abstract class BaseEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}
