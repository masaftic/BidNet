namespace BidNet.Shared.Models;

public record PaginatedResponse<T>
{
    public T[] Items { get; init; } = [];
    public int PageIndex { get; init; }
    public int TotalCount { get; init; }
    public int PageSize { get; init; }
}
