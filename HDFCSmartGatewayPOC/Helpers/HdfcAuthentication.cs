
using System;
using System.Net.Http.Headers;
using System.Text;

namespace HDFCSmartGatewayPOC.Helpers
{
	public static class HdfcAuthentication
	{
		public static AuthenticationHeaderValue CreateBasicAuth(
			string username,
			string password)
		{
			if (string.IsNullOrWhiteSpace(username))
				throw new ArgumentException("Username is required.");

			if (password == null)
				throw new ArgumentNullException(nameof(password));

			string credentials = username + ":" + password;

			string encodedCredentials = Convert.ToBase64String(
				Encoding.UTF8.GetBytes(credentials)
			);

			return new AuthenticationHeaderValue(
				"Basic",
				encodedCredentials
			);
		}
	}
}
