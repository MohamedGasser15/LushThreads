using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Order
{
    public class OrderHeaderDto
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public string CustomerName { get; set; }       // from ApplicationUser.Name
        public string CustomerEmail { get; set; }      // from ApplicationUser.Email
        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }
        public double OrderTotal { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
        public string PhoneNumber { get; set; }
        public string StreetAddress { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Name { get; set; }
    }
}
