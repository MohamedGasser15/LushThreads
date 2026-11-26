using LushThreads.Seeder.Services;

namespace LushThreads.Tests
{
    public class SeederTests
    {
        [Fact]
        public void SeedOptions_FromUserCount_ComputesProportionalTargets()
        {
            var options = SeedOptions.FromUserCount(2000);

            Assert.Equal(2000, options.TargetUsers);
            Assert.Equal(300, options.TargetProducts);
            Assert.Equal(1500, options.TargetOrders);
            Assert.Equal(1600, options.TargetDevices);
            Assert.Equal(700, options.TargetPaymentMethods);
            Assert.Equal(800, options.TargetCartItems);
        }
    }
}
