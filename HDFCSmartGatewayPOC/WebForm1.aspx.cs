using System;
using System.Globalization;
using System.Threading;
using System.Web.UI;
using System.Web.Script.Serialization;
using HDFCSmartGatewayPOC.Configuration;
using HDFCSmartGatewayPOC.Models;
using HDFCSmartGatewayPOC.Services;

namespace HDFCSmartGatewayPOC
{
    public partial class WebForm1 : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && String.IsNullOrWhiteSpace(txtOrderId.Text))
                txtOrderId.Text = "ORD" + DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }

        protected async void btnCreateOrder_Click(object sender, EventArgs e)
        {
            lnkHostedPayment.Visible = false;
            try
            {
                decimal amount;
                if (!Decimal.TryParse(txtAmount.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
                    throw new ArgumentException("Enter a valid amount, for example 1.00.");
                var input = new PaymentRequest
                {
                    OrderId = txtOrderId.Text.Trim(), Amount = amount, Currency = "INR",
                    CustomerId = txtCustomerId.Text.Trim(), CustomerEmail = txtEmail.Text.Trim(),
                    CustomerPhone = txtPhone.Text.Trim(), FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(), Description = "HDFC SmartGateway Web Forms UAT test"
                };
                var client = new HdfcPaymentClient(HdfcSettings.Load());
                string json = await client.CreateOrderAsync(input, CancellationToken.None);
                var response = new JavaScriptSerializer().DeserializeObject(json) as System.Collections.Generic.Dictionary<string, object>;
                object linksObject, webUrl;
                if (response != null && response.TryGetValue("payment_links", out linksObject))
                {
                    var links = linksObject as System.Collections.Generic.Dictionary<string, object>;
                    if (links != null && links.TryGetValue("web", out webUrl) && webUrl != null)
                    {
                        lnkHostedPayment.NavigateUrl = Convert.ToString(webUrl, CultureInfo.InvariantCulture);
                        lnkHostedPayment.Visible = true;
                    }
                }
                object status;
                lblMessage.Text = "HDFC Create Order response received. Status: " +
                    (response != null && response.TryGetValue("status", out status) ? Convert.ToString(status) : "received") +
                    ". Open the hosted payment link if returned.";
            }
            catch (Exception ex)
            {
                lblMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }
    }
}
