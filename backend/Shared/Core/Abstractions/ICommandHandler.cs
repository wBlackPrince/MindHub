using CSharpFunctionalExtensions;
using Shared;
using Shared.SharedKernel;

namespace Core.Abstractions;

public interface ICommandHandler<TResponse, in TCommand>
    where TCommand : ICommand
{
    public Task<Result<TResponse, Error>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    public Task<UnitResult<Error>> Handle(TCommand command, CancellationToken cancellationToken);
}