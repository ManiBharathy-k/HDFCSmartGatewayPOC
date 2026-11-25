<%@ Page Language="C#" AutoEventWireup="true" Async="true" CodeBehind="WebForm1.aspx.cs" Inherits="HDFCSmartGatewayPOC.WebForm1" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>HDFC SmartGateway UAT POC</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <style>
        body{font-family:Segoe UI,Arial,sans-serif;background:#f4f6fa;margin:0;padding:24px;color:#172033}
        .panel{max-width:720px;margin:24px auto;background:#fff;padding:28px;border-radius:12px;box-shadow:0 8px 30px rgba(20,35,60,.08)}
        label{display:block;margin:14px 0 6px;font-weight:600} input{width:100%;box-sizing:border-box;padding:11px;border:1px solid #cbd3df;border-radius:7px}
        .btn{margin-top:20px;padding:12px 20px;border:0;border-radius:7px;background:#0759b5;color:#fff;font-weight:600;cursor:pointer}
        .result{display:block;white-space:pre-wrap;overflow-wrap:anywhere;background:#f3f6fa;padding:14px;border-radius:7px;margin-top:18px}
        .muted{color:#64748b;font-size:14px}
    </style>
</head>
<body>
<form id="form1" runat="server">
<div class="panel">
    <h1>HDFC SmartGateway</h1>
    <p class="muted">ASP.NET Web Forms · .NET Framework 4.8 · UAT only</p>
    <label>Order ID</label><asp:TextBox ID="txtOrderId" runat="server" />
    <label>Amount (INR)</label><asp:TextBox ID="txtAmount" runat="server" Text="1.00" />
    <label>Customer ID</label><asp:TextBox ID="txtCustomerId" runat="server" Text="CUSTOMER1003" />
    <label>Customer email</label><asp:TextBox ID="txtEmail" runat="server" Text="test@example.com" />
    <label>Customer phone</label><asp:TextBox ID="txtPhone" runat="server" Text="9876543219" />
    <label>First name</label><asp:TextBox ID="txtFirstName" runat="server" Text="Test" />
    <label>Last name</label><asp:TextBox ID="txtLastName" runat="server" Text="Customer" />
    <asp:Button ID="btnCreateOrder" runat="server" Text="Create HDFC Order" CssClass="btn" OnClick="btnCreateOrder_Click" />
    <asp:Label ID="lblMessage" runat="server" CssClass="result" />
    <asp:HyperLink ID="lnkHostedPayment" runat="server" Target="_blank" Text="Open HDFC hosted payment page" Visible="false" />
    <p class="muted">Hosted payment link is shown as a UAT fallback. Hypercheckout Web SDK launch requires the exact merchant Web SDK script and initialization instructions.</p>
</div>
</form>
</body>
</html>
