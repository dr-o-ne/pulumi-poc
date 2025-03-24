using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FunctionA;

public sealed class FunctionA
{
    [Function("FunctionA")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest request) => new OkObjectResult("FunctionA");
}
