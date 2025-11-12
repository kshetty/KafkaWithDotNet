using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UserService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
  protected readonly IMediator Mediator;

  protected BaseController(IMediator mediator)
  {
    Mediator = mediator;
  }

  protected IActionResult HandleResult<T>(T result)
  {
    if (result == null)
    {
      return NotFound();
    }

    return Ok(result);
  }

  protected IActionResult Created<T>(T result, string? location = null)
  {
    if (location != null)
    {
      return CreatedAtAction(location, result);
    }

    return CreatedAtAction(null, result);
  }
}
