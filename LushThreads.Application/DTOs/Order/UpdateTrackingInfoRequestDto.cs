using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Order
{
    public class UpdateTrackingInfoRequestDto
    {
        [Required]
        public int OrderId { get; set; }
        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
    }
}
