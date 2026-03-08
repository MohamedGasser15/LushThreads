using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.Entites
{
    public class Brand
    {
        [Key]
        public int Brand_Id { get; set; }
        [Required]
        public string Brand_Name { get; set; }
    }
}
