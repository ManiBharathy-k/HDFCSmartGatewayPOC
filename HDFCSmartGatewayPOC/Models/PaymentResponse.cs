using System;
using System.Collections.Generic;

namespace HDFCSmartGatewayPOC.Models
{
    // Typed overview of Create Order fields. The client returns raw JSON too,
    // preserving all SDK payload properties not modelled here.
    public sealed class PaymentResponse
    {
        public string Status { get; set; }
        public string Id { get; set; }
        public string OrderId { get; set; }
        public PaymentLinks PaymentLinks { get; set; }
        public Dictionary<string, object> SdkPayload { get; set; }
        public DateTime? OrderExpiry { get; set; }
    }
    public sealed class PaymentLinks
    {
        public string Web { get; set; }
        public string Mobile { get; set; }
        public string Iframe { get; set; }
        public DateTime? Expiry { get; set; }
    }
}
