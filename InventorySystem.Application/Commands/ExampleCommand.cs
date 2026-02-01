using MediatR;

namespace InventorySystem.Application.Commands;

/// <summary>
/// Ejemplo de Command con su validator.
/// Elimina este archivo cuando crees tus propios Commands.
/// </summary>
public sealed record ExampleCommand(string Name, int Quantity) : IRequest<Guid>;
