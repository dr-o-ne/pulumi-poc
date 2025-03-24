using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FunctionB;

public sealed class FunctionB
{
    [Function("FunctionB")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest request) => new OkObjectResult("FunctionB");
}
