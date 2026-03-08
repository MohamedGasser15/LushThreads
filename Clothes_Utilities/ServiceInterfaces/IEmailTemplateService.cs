using LushThreads.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    public interface IEmailTemplateService
    {
        string GetOrderDeliveredEmail(ApplicationUser user, int orderNumber, string orderLink);
        string GetOrderShippedEmail(ApplicationUser user, int orderNumber, string orderLink);
        string GetOrderInProcessEmail(ApplicationUser user, int orderNumber, string orderLink);
        string GetOrderCancelledEmail(ApplicationUser user, int orderNumber, string orderLink);
    }
}
