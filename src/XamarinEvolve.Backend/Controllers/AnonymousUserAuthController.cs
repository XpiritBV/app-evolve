using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Login;
using Newtonsoft.Json.Linq;
using XamarinEvolve.Backend.Identity;
using System.IdentityModel.Tokens;

namespace XamarinEvolve.Backend.Controllers
{
    public class AnonymousUserAuthController : ApiController
    {
        private const string AuthSigningKeyVariableName = "WEBSITE_AUTH_SIGNING_KEY";
        private const string HostNameVariableName = "WEBSITE_HOSTNAME";

        public IHttpActionResult Post([FromBody] JObject assertion)
        {
            var userId = Guid.NewGuid().ToString();

            var cred = assertion.ToObject<AnonymousUserCredentials>();
            if (!string.IsNullOrEmpty(cred.AnonymousUserId))
            {
                Guid impersonate;
                if (Guid.TryParse(cred.AnonymousUserId, out impersonate))
                {
                    userId = impersonate.ToString();
                }
            }

            IEnumerable<Claim> claims = GetAccountClaims(userId);
            string websiteUri = $"https://{WebsiteHostName}/";

            var token = AppServiceLoginHandler.CreateToken(claims, TokenSigningKey, websiteUri, websiteUri, TimeSpan.FromDays(100));

            return Ok(new LoginResult { RawToken = token.RawData, User = new User { UserId = userId } });
        }

        private IEnumerable<Claim> GetAccountClaims(string user) => new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user),
                new Claim(JwtRegisteredClaimNames.GivenName, "anonymous"),
                new Claim(JwtRegisteredClaimNames.FamilyName, "anonymous")
            };

        private string TokenSigningKey => Environment.GetEnvironmentVariable(AuthSigningKeyVariableName) ?? "6879205A9897B2AC46064602B94363C6262E2E40F5A371D1B4B05EB4E852CE5A";

        public string WebsiteHostName => Environment.GetEnvironmentVariable(HostNameVariableName) ?? "localhost";
    }
}
