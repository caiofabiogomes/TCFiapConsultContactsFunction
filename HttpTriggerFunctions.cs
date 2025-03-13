using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TechChallenge.SDK.Persistence;

namespace TCFiapConsultContactsFunction
{
    public class HttpTriggerFunctions
    {
        private readonly ILogger<HttpTriggerFunctions> _logger;
        private readonly ContactsService _contactsService;

        public HttpTriggerFunctions(ILogger<HttpTriggerFunctions> logger, IContactRepository contactRepository)
        {
            _logger = logger;
            _contactsService = new ContactsService(logger, contactRepository);
        }

        [Function("GetContactsFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            if (_contactsService.GetContactsList(req, out var contacts, out var run))
                return run;

            return new OkObjectResult(contacts);
        }
    }
}
