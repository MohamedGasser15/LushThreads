namespace LushThreads.Domain.Constants
{
    /// <summary>
    /// Static details class containing application-wide constants.
    /// </summary>
    public static class SD
    {
        #region Roles

        public const string User = "User";

        public const string Admin = "Admin";

        #endregion

        #region TempData Keys

        public const string Success = "Success";

        public const string Error = "Error";

        #endregion

        #region Order Statuses

        public const string StatusPending = "Pending";

        public const string StatusApproved = "Approved";

        public const string StatusInProcess = "Processing";

        public const string StatusShipped = "Shipped";

        public const string StatusCancelled = "Cancelled";

        public const string StatusRefunded = "Refunded";

        public static string StatusDelivered = "Delivered";

        #endregion

        #region Payment Statuses

        public const string PaymentStatusPending = "Pending";

        public const string PaymentStatusApproved = "Approved";

        public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";

        public const string PaymentStatusRejected = "Rejected";

        #endregion
    }
}