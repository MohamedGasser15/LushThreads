using LushThreads.Domain.Entites;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(ApplicationUser user, IList<string> roles);
    }
}