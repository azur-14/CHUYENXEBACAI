using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CHUYENXEBACAI.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : ControllerBase
{
    [Route("/error")]
    public IActionResult Handle()
    {
        var ex = HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error;
        return Problem(title: "Unexpected error", detail: ex?.Message, statusCode: 500);
    }
}
                                                                       