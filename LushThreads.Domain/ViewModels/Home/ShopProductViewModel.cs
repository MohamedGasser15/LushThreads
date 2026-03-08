using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.Home
{
    /// <summary>
    /// ViewModel for displaying products in the shop page.
    /// </summary>
    public class ShopProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImgUrl { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public bool IsFeatured { get; set; }
        public int ProductRating { get; set; }
        public decimal ProductPrice { get; set; }
        public List<string> AvailableSizes { get; set; }
    }
}
