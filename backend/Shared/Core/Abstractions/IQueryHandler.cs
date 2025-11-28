using CSharpFunctionalExtensions;
using Shared;
using Shared.SharedKernel;

namespace Core.Abstractions;

public interface IQueryHandler<TResponse, in TQuery>
    where TQuery : IQuery
{
    Task<TResponse> Handle(TQuery command, CancellationToken cancellationToken);
}

public interface IQueryHandlerWithResult<TResponse, in TQuery>
    where TQuery : IQuery
{
    Task<Result<TResponse, Error>> Handle(TQuery command, CancellationToken cancellationToken);
}