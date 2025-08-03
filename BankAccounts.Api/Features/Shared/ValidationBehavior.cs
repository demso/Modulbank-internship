using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace BankAccounts.Api.Features.Shared;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest :IRequest<TResponse>
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var failure = validators
            .Select(validator => validator.Validate(context))
            .SelectMany(result => result.Errors)
            .FirstOrDefault(failure => failure != null); //вернем только первую ошибку

        if (failure is not null)
        {
            throw new ValidationException(new List<ValidationFailure> { failure });
        }

        return next(cancellationToken);
    }
}