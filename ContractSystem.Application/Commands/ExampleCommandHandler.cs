using MediatR;

namespace ContractSystem.Application.Commands;

/// <summary>
/// Handler de ejemplo. Elimina junto con ExampleCommand y ExampleCommandValidator.
/// </summary>
internal sealed class ExampleCommandHandler : IRequestHandler<ExampleCommand, Guid>
{
    public Task<Guid> Handle(ExampleCommand request, CancellationToken cancellationToken)
    {
        // Ejemplo: devuelve un GUID simulado
        return Task.FromResult(Guid.NewGuid());
    }
}
