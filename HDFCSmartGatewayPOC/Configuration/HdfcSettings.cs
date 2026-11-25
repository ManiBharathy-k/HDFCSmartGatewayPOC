
using System;
using System.Configuration;
using System.IO;
using System.Web;

namespace HDFCSmartGatewayPOC.Configuration
{
    public sealed class HdfcSettings
    {
        public string ApiKey { get; private set; }
        public string MerchantId { get; private set; }
        public string BaseUrl { get; private set; }
        public string PaymentPageClientId { get; private set; }
        public string ResponseKey { get; private set; }
        public string ReturnUrl { get; private set; }
        public bool EnableLogging { get; private set; }
        public int TimeoutSeconds { get; private set; }

        public static HdfcSettings Load()
        {
            // Load .env before reading configuration values.
            LoadDotEnv();

            return new HdfcSettings
            {
                ApiKey = GetValue("HDFC_API_KEY", ""),
                MerchantId = GetValue("HDFC_MERCHANT_ID", "SG6695"),
                BaseUrl = GetValue(
                    "HDFC_BASE_URL",
                    "https://smartgateway.hdfcuat.bank.in"
                ),
                PaymentPageClientId = GetValue(
                    "HDFC_PAYMENT_PAGE_CLIENT_ID",
                    "hdfcmaster"
                ),
                ResponseKey = GetValue("HDFC_RESPONSE_KEY", ""),
                ReturnUrl = GetValue("HDFC_RETURN_URL", ""),
                EnableLogging = GetValue(
                    "HDFC_ENABLE_LOGGING",
                    "false"
                ).Equals(
                    "true",
                    StringComparison.OrdinalIgnoreCase
                ),
                TimeoutSeconds = ParseTimeout(
                    GetValue("HDFC_TIMEOUT_SECONDS", "30")
                )
            };
        }

        private static void LoadDotEnv()
        {
            string envPath = Path.Combine(
                HttpRuntime.AppDomainAppPath,
                ".env"
            );

            if (!File.Exists(envPath))
                return;

            foreach (string line in File.ReadAllLines(envPath))
            {
                string value = line.Trim();

                if (String.IsNullOrWhiteSpace(value) ||
                    value.StartsWith("#"))
                    continue;

                int separator = value.IndexOf('=');

                if (separator <= 0)
                    continue;

                string key = value.Substring(0, separator).Trim();
                string data = value.Substring(separator + 1).Trim();

                if (String.IsNullOrWhiteSpace(key))
                    continue;

                // Remove surrounding single or double quotes.
                if (data.Length >= 2 &&
                    ((data[0] == '"' &&
                      data[data.Length - 1] == '"') ||
                     (data[0] == '\'' &&
                      data[data.Length - 1] == '\'')))
                {
                    data = data.Substring(1, data.Length - 2);
                }

                // Existing Windows environment variables take priority.
                if (String.IsNullOrWhiteSpace(
                    Environment.GetEnvironmentVariable(key)))
                {
                    Environment.SetEnvironmentVariable(
                        key,
                        data,
                        EnvironmentVariableTarget.Process
                    );
                }
            }
        }

        public void Validate()
        {
            Uri uri;

            if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out uri) ||
                uri.Scheme != Uri.UriSchemeHttps ||
                !String.Equals(
                    uri.Host,
                    "smartgateway.hdfcuat.bank.in",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "This POC is sandbox-only. HDFC_BASE_URL must be " +
                    "https://smartgateway.hdfcuat.bank.in."
                );
            }

            if (String.IsNullOrWhiteSpace(ApiKey))
                throw new InvalidOperationException(
                    "Set HDFC_API_KEY in your .env file or environment variables."
                );

            if (String.IsNullOrWhiteSpace(MerchantId))
                throw new InvalidOperationException(
                    "HDFC_MERCHANT_ID is required."
                );

            if (String.IsNullOrWhiteSpace(PaymentPageClientId))
                throw new InvalidOperationException(
                    "HDFC_PAYMENT_PAGE_CLIENT_ID is required."
                );
        }

        private static string GetValue(string key, string fallback)
        {
            string value = Environment.GetEnvironmentVariable(key);

            if (String.IsNullOrWhiteSpace(value))
                value = ConfigurationManager.AppSettings[key];

            return String.IsNullOrWhiteSpace(value)
                ? fallback
                : value;
        }

        private static int ParseTimeout(string value)
        {
            int seconds;

            if (!Int32.TryParse(value, out seconds))
                seconds = 30;

            return Math.Max(5, Math.Min(120, seconds));
        }
    }
}
