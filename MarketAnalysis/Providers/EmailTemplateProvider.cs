using MarketAnalysis.Models;
using MarketAnalysis.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class EmailTemplateProvider
    {
        private readonly IRepository<EmailTemplate> _emailTemplateRepository;

        public EmailTemplateProvider(IRepository<EmailTemplate> emailTemplateRepository)
        {
            _emailTemplateRepository = emailTemplateRepository;
        }

        public async Task<string> GetEmailTemplate()
        {
            var templates = await _emailTemplateRepository.Get();
            return templates.First().Body;
        }

        public Task<IEnumerable<RecipientDetails>> GetEmailRecipients()
        {
            return Task.FromResult((IEnumerable<RecipientDetails>)new[]
            {
                new RecipientDetails
                {
                    Date = DateTime.Now,
                    Name = "Client Name",
                    Number = "000001",
                    Email = "client@email.com"
                },
            });
        }
    }
}
