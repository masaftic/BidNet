using BidNet.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BidNet.Data.Persistence.Converters;

public class UserIdConverter : ValueConverter<UserId, Guid>
{
    public UserIdConverter() : base(v => v.Value, v => new UserId(v))
    {
    }
}

public class AuctionIdConverter : ValueConverter<AuctionId, Guid>
{
    public AuctionIdConverter() : base(v => v.Value, v => new AuctionId(v))
    {
    }
}

public class BidIdConverter : ValueConverter<BidId, Guid>
{
    public BidIdConverter() : base(v => v.Value, v => new BidId(v))
    {
    }
}

public class RefreshTokenIdConverter : ValueConverter<RefreshTokenId, Guid>
{
    public RefreshTokenIdConverter() : base(v => v.Value, v => new RefreshTokenId(v))
    {
    }
}

public class PaymentTransactionIdConverter : ValueConverter<PaymentTransactionId, Guid>
{
    public PaymentTransactionIdConverter() : base(v => v.Value, v => new PaymentTransactionId(v))
    {
    }
}

public class AuctionEventLogIdConverter : ValueConverter<AuctionEventLogId, Guid>
{
    public AuctionEventLogIdConverter() : base(v => v.Value, v => new AuctionEventLogId(v))
    {
    }
}

public class WalletIdConverter : ValueConverter<WalletId, Guid>
{
    public WalletIdConverter() : base(v => v.Value, v => new WalletId(v))
    {
    }
}

public class WalletTransactionIdConverter : ValueConverter<WalletTransactionId, Guid>
{
    public WalletTransactionIdConverter() : base(v => v.Value, v => new WalletTransactionId(v))
    {
    }
}

public class AuctionImageIdConverter : ValueConverter<AuctionImageId, Guid>
{
    public AuctionImageIdConverter()
        : base(
            v => v.Value,
            v => new AuctionImageId(v))
    {
    }
}
