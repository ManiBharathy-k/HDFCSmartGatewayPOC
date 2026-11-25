namespace HDFCSmartGatewayPOC.Models
{
    public sealed class PaymentRequest
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string CustomerId { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string ReturnUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
    }
}
