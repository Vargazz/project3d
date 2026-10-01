using FluentValidation;
using Project3D.Application.DTOs.CustomRequests;

namespace Project3D.Application.Validators;

public class CreateCustomRequestValidator : AbstractValidator<CreateCustomRequestRequest>
{
    public CreateCustomRequestValidator()
    {
        RuleFor(x => x.ProductType)
            .NotEmpty().WithMessage("Tipo de produto é obrigatório.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MinimumLength(20).WithMessage("Descreva melhor o que você deseja (mínimo 20 caracteres).");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");

        RuleFor(x => x.ContactName)
            .NotEmpty().WithMessage("Nome de contato é obrigatório.");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.ContactWhatsApp)
            .Matches(@"^[\d\s\(\)\-\+]+$").WithMessage("WhatsApp inválido.")
            .When(x => !string.IsNullOrEmpty(x.ContactWhatsApp));
    }
}

public class CreateQuoteRequestValidator : AbstractValidator<CreateQuoteRequest>
{
    public CreateQuoteRequestValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Valor do orçamento deve ser maior que zero.");

        RuleFor(x => x.DeadlineDays)
            .GreaterThan(0).WithMessage("Prazo deve ser maior que zero.");
    }
}
