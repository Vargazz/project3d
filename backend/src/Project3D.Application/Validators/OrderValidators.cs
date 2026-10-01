using FluentValidation;
using Project3D.Application.DTOs.Orders;

namespace Project3D.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("O pedido deve ter ao menos um item.");

        RuleForEach(x => x.Items).SetValidator(new CartItemRequestValidator());

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Nome do cliente é obrigatório.");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.CustomerPhone)
            .NotEmpty().WithMessage("Telefone é obrigatório.");

        // Deve ter endereço existente OU novo endereço
        RuleFor(x => x)
            .Must(x => x.AddressId.HasValue || x.NewAddress is not null)
            .WithMessage("Informe um endereço de entrega.");
    }
}

public class CartItemRequestValidator : AbstractValidator<CartItemRequest>
{
    public CartItemRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");
    }
}

public class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.ZipCode).NotEmpty().Length(8, 9).WithMessage("CEP inválido.");
        RuleFor(x => x.State).NotEmpty().Length(2, 2).WithMessage("Estado inválido (use a sigla, ex: SP).");
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Neighborhood).NotEmpty();
        RuleFor(x => x.Street).NotEmpty();
        RuleFor(x => x.Number).NotEmpty();
    }
}
