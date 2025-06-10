using FluentValidation;

namespace Order.Application.Commands.RegisterOrder
{
    public class RegisterOrderValidator : AbstractValidator<RegisterOrderCommand>
    {
        public RegisterOrderValidator()
        {
            RuleFor(x => x.ExternalId)
                .NotEmpty().WithMessage("ExternalId is required");

            RuleFor(x => x.Products)
                .NotEmpty().WithMessage("At least one product is required");

            RuleForEach(x => x.Products).ChildRules(product =>
            {
                product.RuleFor(p => p.Name)
                    .NotEmpty().WithMessage("Product name is required");

                product.RuleFor(p => p.Price)
                    .GreaterThan(0).WithMessage("Product price must be greater than zero");

                product.RuleFor(p => p.Quantity)
                    .GreaterThan(0).WithMessage("Product quantity must be greater than zero");
            });
        }
    }
}
