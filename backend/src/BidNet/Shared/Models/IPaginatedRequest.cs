namespace BidNet.Shared.Models;

public interface IPaginatedRequest
{
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
}
