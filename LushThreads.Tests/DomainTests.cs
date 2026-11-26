using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;

namespace LushThreads.Tests
{
    public class DomainTests
    {
        [Fact]
        public void Product_IsNewArrival_ReturnsTrueWithinDefault30Days()
        {
            var recent = new Product { DateAdded = DateTime.Now.AddDays(-5) };
            var old = new Product { DateAdded = DateTime.Now.AddDays(-45) };

            Assert.True(recent.IsNewArrival);
            Assert.False(old.IsNewArrival);
        }

        [Fact]
        public void Product_IsNewArrival_RespectsCustomDurationDays()
        {
            var product = new Product
            {
                DateAdded = DateTime.Now.AddDays(-10),
                NewArrivalDurationDays = 7
            };

            Assert.False(product.IsNewArrival);
        }

        [Fact]
        public void CartItem_Total_ComputesQuantityTimesPrice()
        {
            var item = new CartItem
            {
                Quantity = 3,
                Product = new Product { Product_Price = 25.50m }
            };

            Assert.Equal(76.50m, item.Total);
        }

        [Fact]
        public void SD_RolesAndStatuses_AreDefined()
        {
            Assert.Equal("Admin", SD.Admin);
            Assert.Equal("User", SD.User);
            Assert.Equal("Approved", SD.StatusApproved);
            Assert.Equal("Delivered", SD.StatusDelivered);
        }
    }
}
