using FluentValidation;

namespace Order.Application.Queries.GetOrders
{
    public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
    {
        public GetOrdersQueryValidator()
        {
            // No specific validation rules required
            // OrderId and ExternalId are optional filters
        }
    }
}
