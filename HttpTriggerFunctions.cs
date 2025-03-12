using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TechChallenge.SDK.Models;
using TechChallenge.SDK.Persistence;

namespace TCFiapConsultContactsFunction
{
    public class HttpTriggerFunctions
    {
        private readonly ILogger<HttpTriggerFunctions> _logger;
        private readonly IContactRepository _contactRepository;

        public HttpTriggerFunctions(ILogger<HttpTriggerFunctions> logger, IContactRepository contactRepository)
        {
            _logger = logger;
            _contactRepository = contactRepository;
        }

        [Function("GetContactsFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            if (GetContactsList(req, out var contacts, out var run))
                return run;

            return new OkObjectResult(contacts);
        }

        private bool GetContactsList(HttpRequest req, out List<Contact> contacts, out IActionResult run)
        {
            contacts = new List<Contact>();
            run = new BadRequestObjectResult("Um erro ocorreu.");

            var contactsQueryable = _contactRepository.Query();

            if (req.Query.TryGetValue("Id", out var idValue))
            {
                if (Guid.TryParse(idValue, out var idGuid))
                {
                    contactsQueryable = contactsQueryable.Where(i => i.Id == idGuid);
                    _logger.LogInformation($"Filtrando contatos pelo Id: {idGuid}");
                }
                else
                {
                    _logger.LogWarning("Formato de Id inválido.");
                    run = new BadRequestObjectResult("Formato de Id inválido.");
                    return true;
                }
            }

            if (req.Query.TryGetValue("Email", out var email))
            {
                contactsQueryable = contactsQueryable.Where(i => i.EmailAddress.Equals(email, StringComparison.OrdinalIgnoreCase));
                _logger.LogInformation($"Filtrando contatos pelo Email: {email}");
            }

            if (req.Query.TryGetValue("PhoneNumber", out var phoneValue))
            {
                if (int.TryParse(phoneValue, out var phoneNumber))
                {
                    contactsQueryable = contactsQueryable.Where(i => i.PhoneNumber == phoneNumber);
                    _logger.LogInformation($"Filtrando contatos pelo PhoneNumber: {phoneNumber}");
                }
                else
                {
                    _logger.LogWarning("Formato de PhoneNumber inválido.");
                    run = new BadRequestObjectResult("Formato de PhoneNumber inválido.");
                    return true;
                }
            }

            if (req.Query.TryGetValue("FirstName", out var firstName) && req.Query.TryGetValue("LastName", out var lastName))
            {
                contactsQueryable = contactsQueryable.Where(i =>
                    i.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                    i.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));

                _logger.LogInformation($"Filtrando contatos pelo FirstName: {firstName} e LastName: {lastName}");
            }

            contacts = contactsQueryable.ToList();
            _logger.LogInformation($"Retornando {contacts.Count} contato(s).");
            return false;
        }
    }
}
