using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Features
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CustomControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private IMediator _mediator;

        protected IMediator Mediator =>
            _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
    }
}
