using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using XamarinEvolve.DataObjects;

namespace XamarinEvolve.DataStore.Azure
{
	public static class MobileServiceUserExtensions
	{
		public static AccountResponse AsAccount(this MobileServiceUser source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			IDictionary<string, string> claims = JwtUtility.GetClaims(source.MobileServiceAuthenticationToken);

			var account = new AccountResponse();
			account.Success = true;
			account.User = new User
			{
				Email = claims[JwtClaimNames.Subject],
				FirstName = claims[JwtClaimNames.GivenName],
				LastName = claims[JwtClaimNames.FamilyName]
			};

			return account;
		}
	}
}
