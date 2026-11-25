using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using HDFCSmartGatewayPOC.Configuration;
using HDFCSmartGatewayPOC.Models;

namespace HDFCSmartGatewayPOC.Services
{
    /// <summary>Server-side HDFC SmartGateway Basic Auth client ported from supplied .NET 8 POC.</summary>
    public sealed class HdfcPaymentClient
    {
        private readonly HdfcSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly JavaScriptSerializer _serializer;

        public HdfcPaymentClient(HdfcSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            settings.Validate();
            _settings = settings;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds) };
            _serializer = new JavaScriptSerializer { MaxJsonLength = 1024 * 1024 };
        }

        public async Task<string> CreateOrderAsync(PaymentRequest input, CancellationToken cancellationToken)
        {
            if (input == null) throw new ArgumentNullException("input");
            ValidateOrder(input);
            var requestBody = new System.Collections.Generic.Dictionary<string, object>
            {
                { "order_id", input.OrderId },
                { "amount", input.Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) },
                { "currency", String.IsNullOrWhiteSpace(input.Currency) ? "INR" : input.Currency.ToUpperInvariant() },
                { "customer_id", input.CustomerId ?? "" },
                { "payment_page_client_id", _settings.PaymentPageClientId },
                { "action", "paymentPage" },
                { "return_url", String.IsNullOrWhiteSpace(input.ReturnUrl) ? _settings.ReturnUrl : input.ReturnUrl },
                { "customer_email", input.CustomerEmail },
                { "customer_phone", input.CustomerPhone },
                { "first_name", input.FirstName },
                { "last_name", input.LastName },
                { "description", input.Description }
            };
            string body = _serializer.Serialize(requestBody);
            string routingId = String.IsNullOrWhiteSpace(input.CustomerId) ? input.OrderId : input.CustomerId;
            return await SendAsync(HttpMethod.Post, "/session", body, routingId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetOrderStatusAsync(string orderId, string customerId, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(orderId) || orderId.Length > 20)
                throw new ArgumentException("Order ID is required and must not exceed 20 characters.", "orderId");
            if (String.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer/routing ID is required.", "customerId");
            return await SendAsync(HttpMethod.Get, "/orders/" + Uri.EscapeDataString(orderId), null, customerId, cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> SendAsync(HttpMethod method, string path, string body, string routingId, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(method, new Uri(_settings.BaseUrl.TrimEnd('/') + path)))
            {
                string basic = Convert.ToBase64String(Encoding.UTF8.GetBytes(_settings.ApiKey + ":"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
                request.Headers.TryAddWithoutValidation("x-merchantid", _settings.MerchantId);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!String.IsNullOrWhiteSpace(routingId)) request.Headers.TryAddWithoutValidation("x-routing-id", routingId);
                if (body != null) request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                try
                {
                    using (var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                    {
                        string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (_settings.EnableLogging)
                            System.Diagnostics.Trace.TraceInformation("HDFC {0} {1} -> {2}", method, path, (int)response.StatusCode);
                        if (!response.IsSuccessStatusCode) throw new HdfcApiException((int)response.StatusCode, responseBody);
                        return responseBody; // Keep the complete response, including sdk_payload.
                    }
                }
                catch (TaskCanceledException ex) { throw new TimeoutException("HDFC sandbox request timed out.", ex); }
                catch (HttpRequestException ex) { throw new InvalidOperationException("Could not connect to HDFC sandbox. Check URL, TLS and network access.", ex); }
            }
        }

        private void ValidateOrder(PaymentRequest input)
        {
            if (String.IsNullOrWhiteSpace(input.OrderId) || input.OrderId.Length > 20 ||
                !System.Text.RegularExpressions.Regex.IsMatch(input.OrderId, "^[A-Za-z0-9]+$"))
                throw new ArgumentException("OrderId is required, alphanumeric and no longer than 20 characters.");
            if (input.Amount <= 0 || Decimal.Round(input.Amount, 2) != input.Amount)
                throw new ArgumentException("Amount must be positive with no more than two decimal places.");
            if (String.IsNullOrWhiteSpace(input.CustomerEmail) && String.IsNullOrWhiteSpace(input.CustomerPhone))
                throw new ArgumentException("Provide at least customer email or phone.");
            string returnUrl = String.IsNullOrWhiteSpace(input.ReturnUrl) ? _settings.ReturnUrl : input.ReturnUrl;
            Uri parsed;
            if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out parsed) || parsed.Scheme != Uri.UriSchemeHttps ||
                !String.IsNullOrEmpty(parsed.Query) || !String.IsNullOrEmpty(parsed.UserInfo))
                throw new ArgumentException("Return URL must be an absolute HTTPS URL without query parameters.");
        }
    }

    public sealed class HdfcApiException : Exception
    {
        public int HttpStatusCode { get; private set; }
        public string ResponseBody { get; private set; }
        public HdfcApiException(int statusCode, string responseBody) : base("HDFC returned HTTP " + statusCode + ".")
        { HttpStatusCode = statusCode; ResponseBody = responseBody; }
    }
}
