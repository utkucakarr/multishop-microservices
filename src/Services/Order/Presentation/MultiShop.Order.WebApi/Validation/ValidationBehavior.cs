using FluentValidation;
using MediatR;
using MultiShop.BuildingBlocks.Exceptions;

namespace MultiShop.Order.WebApi.Validation
{
    /// <summary>
    /// MediatR pipeline'ı: isteğin doğrulayıcıları (Application/Validators) handler'dan önce çalışır; hata varsa
    /// <see cref="ValidationFailedException"/> fırlatılır, middleware alan bazlı 400 döner (INF-05).
    /// Application katmanı ASP.NET'e bağımlı olmasın diye exception'ı burada (WebApi) çeviriyoruz.
    /// </summary>
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var errors = results
                .SelectMany(x => x.Errors)
                .GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).Distinct().ToArray());

            if (errors.Count > 0)
                throw new ValidationFailedException(errors);

            return await next();
        }
    }
}
