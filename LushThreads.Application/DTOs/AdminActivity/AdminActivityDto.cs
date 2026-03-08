using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.AdminActivity
{
    public class AdminActivityDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } 
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime ActivityDate { get; set; }
        public string IpAddress { get; set; }
    }
}
