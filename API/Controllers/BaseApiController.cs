using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        private IMediator _mediator;

        //"protected" means available to any derived classes and the BaseApi class itself.
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices
        .GetService<IMediator>();     
    }
}