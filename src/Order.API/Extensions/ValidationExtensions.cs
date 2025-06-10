namespace Order.API.Extensions
{
    public static class ValidationExtensions
    {
        public static object ToResponse(this IEnumerable<FluentValidation.Results.ValidationFailure> failures)
        {
            return new
            {
                Success = false,
                Message = "Validation failed",
                Errors = failures.Select(f => new
                {
                    Property = f.PropertyName,
                    Error = f.ErrorMessage
                }).ToList()
            };
        }
    }
}
