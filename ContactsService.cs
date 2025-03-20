using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TechChallenge.SDK.Domain.Models;
using TechChallenge.SDK.Infrastructure.Persistence;

namespace TCFiapConsultContactsFunction
{
    public class ContactsService(ILogger<HttpTriggerFunctions> logger, IContactRepository contactRepository)
    {
        public bool GetContactsList(HttpRequest req, out List<Contact> contacts, out IActionResult run)
        {
            contacts = new List<Contact>();
            run = new BadRequestObjectResult("Um erro ocorreu.");

            var contactsQueryable = contactRepository.Query();

            if (!TryFilterById(req, ref contactsQueryable, out var error))
            {
                run = error;
                return true;
            }

            FilterByEmail(req, ref contactsQueryable);

            if (!TryFilterByPhoneNumber(req, ref contactsQueryable, out error))
            {
                run = error;
                return true;
            }

            if (!TryFilterByPhoneDdd(req, ref contactsQueryable, out error))
            {
                run = error;
                return true;
            }

            FilterByName(req, ref contactsQueryable);

            contacts = contactsQueryable.ToList();
            logger.LogInformation($"Retornando {contacts.Count} contato(s).");
            return false;
        }

        #region Private Methods

        private bool TryFilterById(HttpRequest req, ref IQueryable<Contact> query, out IActionResult error)
        {
            error = null;

            if (!req.Query.TryGetValue("Id", out var idValue))
                return true;

            if (Guid.TryParse(idValue, out var idGuid))
            {
                query = query.Where(i => i.Id == idGuid);
                logger.LogInformation($"Filtrando contatos pelo Id: {idGuid}");
            }
            else
            {
                logger.LogWarning("Formato de Id inválido.");
                error = new BadRequestObjectResult("Formato de Id inválido.");
                return false;
            }
            return true;
        }

        private void FilterByEmail(HttpRequest req, ref IQueryable<Contact> query)
        {
            if (req.Query.TryGetValue("Email", out var email))
            {
                query = query.Where(i => i.Email.Endereco.Equals(email, StringComparison.OrdinalIgnoreCase));
                logger.LogInformation($"Filtrando contatos pelo Email: {email}");
            }
        }

        private bool TryFilterByPhoneNumber(HttpRequest req, ref IQueryable<Contact> query, out IActionResult error)
        {
            error = null;
            if (req.Query.TryGetValue("PhoneNumber", out var phoneValue))
            {
                if (int.TryParse(phoneValue, out var phoneNumber))
                {
                    query = query.Where(i => i.Phone.Number == phoneNumber);
                    logger.LogInformation($"Filtrando contatos pelo PhoneNumber: {phoneNumber}");
                }
                else
                {
                    logger.LogWarning("Formato de PhoneNumber inválido.");
                    error = new BadRequestObjectResult("Formato de PhoneNumber inválido.");
                    return false;
                }
            }
            return true;
        }

        private bool TryFilterByPhoneDdd(HttpRequest req, ref IQueryable<Contact> query, out IActionResult error)
        {
            error = null;
            if (req.Query.TryGetValue("PhoneDdd", out var phoneDddStr))
            {
                if (int.TryParse(phoneDddStr, out var phoneDdd))
                {
                    query = query.Where(i => i.Phone.DDD == phoneDdd);
                    logger.LogInformation($"Filtrando contatos pelo PhoneDdd: {phoneDdd}");
                }
                else
                {
                    logger.LogWarning("Formato de PhoneDdd inválido.");
                    error = new BadRequestObjectResult("Formato de PhoneDdd inválido.");
                    return false;
                }
            }
            return true;
        }

        private void FilterByName(HttpRequest req, ref IQueryable<Contact> query)
        {
            if (req.Query.TryGetValue("FirstName", out var firstName) &&
                req.Query.TryGetValue("LastName", out var lastName))
            {
                query = query.Where(i =>
                    i.Name.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                    i.Name.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));

                logger.LogInformation($"Filtrando contatos pelo FirstName: {firstName} e LastName: {lastName}");
            }
        }

        #endregion

    }
}
