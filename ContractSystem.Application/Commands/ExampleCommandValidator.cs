using FluentValidation;

namespace ContractSystem.Application.Commands;

/// <summary>
/// Ejemplo de Validator. Se ejecuta automáticamente antes del Handler.
/// Elimina este archivo cuando crees tus propios Commands.
/// </summary>
public sealed class ExampleCommandValidator : AbstractValidator<ExampleCommand>
{
    public ExampleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");
    }
}
