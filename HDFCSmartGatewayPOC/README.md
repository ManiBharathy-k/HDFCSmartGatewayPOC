# HDFC SmartGateway — ASP.NET Web Forms (.NET Framework 4.8)

This branch ports the server-side API calls from the supplied .NET 8 POC into the existing Visual Studio ASP.NET Web Application (Empty + Web Forms) project.

## Implemented
- Create Order: authenticated POST to HDFC `/session`.
- Order Status: authenticated GET to HDFC `/orders/{orderId}`.
- Basic Auth: API key as username and empty password.
- `x-merchantid` and `x-routing-id` headers.
- Complete raw JSON response is returned by the service so fields such as `sdk_payload` are preserved.
- Web Forms sample page for creating a UAT order.
- UAT-only host validation.
- API key read from environment variable / secure app settings; do not commit secrets.
- Project file includes the settings class.

## Configure on Windows
Set the API key in the current Windows user environment:

```powershell
[Environment]::SetEnvironmentVariable("HDFC_API_KEY", "YOUR_UAT_API_KEY", "User")
```

Close and reopen Visual Studio after setting it. Optional variables: `HDFC_RETURN_URL`, `HDFC_RESPONSE_KEY`, `HDFC_ENABLE_LOGGING`, `HDFC_TIMEOUT_SECONDS`.

Non-secret defaults: merchant `SG6695`, client ID `hdfcmaster`, base URL `https://smartgateway.hdfcuat.bank.in`.

## Run
1. Open `HDFCSmartGatewayPOC.sln` in Visual Studio 2022.
2. Restore NuGet packages.
3. Build the solution.
4. Run with IIS Express and open `WebForm1.aspx`.
5. Create a fresh UAT order. If HDFC returns a hosted payment URL, the page exposes it.

## HDFC Web SDK status
The HDFC integration architecture you shared specifies: Merchant App -> Merchant Server Create Order -> send `sdk_payload` to app -> app invokes `openPaymentPage` in HDFC Frontend SDK -> payment -> return/webhook -> server-side Order Status -> fulfilment.

This branch implements the backend Create Order and Order Status calls. It preserves the entire Create Order JSON response (including `sdk_payload`). It also exposes the hosted payment link as a UAT fallback.

**The browser SDK launch and webhook receiver are not implemented yet.** The supplied original ZIP did not contain the Web SDK script/import or the merchant-specific JavaScript initialization example. We should add these using the exact HDFC Web SDK prerequisites and initialization documentation, rather than inventing a script URL or function signature.

## Security
- This branch is sandbox-only and rejects a base URL other than the HDFC UAT host.
- Never put API keys or response keys in JavaScript, source control, or logs.
- Browser return parameters are not proof of payment. Fetch order status from HDFC server-to-server and only fulfil after checking the authoritative successful status.
- Before production, add authentication, order ownership validation, CSRF protection, idempotency, durable order persistence, webhook verification, rate limiting and redacted audit logging.
- This POC has not been compiled or tested against HDFC from this branch. Run it locally with your UAT credentials.

## Source
Backend request/authentication behaviour was ported from the supplied `HDFC-SmartGateway-DotNet-POC-main` ZIP. Project template remains the existing ASP.NET Web Forms .NET Framework 4.8 project.
