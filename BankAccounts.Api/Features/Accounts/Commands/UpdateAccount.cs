using BankAccounts.Api.Infrastructure;
using MediatR;

namespace BankAccounts.Api.Features.Accounts.Commands;

//public class UpdateAccount
//{
//    public class Command : IRequest
//    {
//        public decimal Balance { get; set; }
//        public decimal? InterestRate { get; set; }
//    }

//    public class Handler(IBankAccountsContext dbContext) : IRequestHandler<Command>
//    {
//        public async Task Handle(Command request, CancellationToken cancellationToken)
//        {
//            request.Re
//            return Response;
//        }
//    }
//}