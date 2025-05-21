using Bidnet.Domain.Entities;

namespace Bidnet.Application.Common.Abstractions;

public interface ICurrentUserService
{
    UserId UserId { get; }
    string UserName { get; }
}