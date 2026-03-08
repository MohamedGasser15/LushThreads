using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Service for retrieving location information from an IP address.
    /// </summary>
    public interface IIpLocationService
    {
        /// <summary>
        /// Gets the location string (City, Country) from an IP address.
        /// </summary>
        Task<string> GetLocationFromIpAsync(string ipAddress);
    }
}
