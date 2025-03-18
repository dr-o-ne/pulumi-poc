using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace TestAzureFunction;

public sealed class Handler
{
    [Function("Handler")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req) => 
        new OkObjectResult("Welcome to Azure Functions!");
}
