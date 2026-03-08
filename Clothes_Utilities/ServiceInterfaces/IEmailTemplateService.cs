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
        string GenerateEmailConfirmationEmail(ApplicationUser user, string code);
        string Generate2FASetupEmail(ApplicationUser user, string code);
        string GeneratePasswordChangeEmail(ApplicationUser user, string ipAddress, string deviceName, DateTime changeTime, string passwordResetLink);
        string GenerateForgotPasswordEmail(
        ApplicationUser user,
        string ipAddress,
        string deviceName,
        DateTime requestTime,
        string code,
        string passwordResetLink);
        string Generate2FACodeEmail(ApplicationUser user, string code, string verificationLink);
    }
}
