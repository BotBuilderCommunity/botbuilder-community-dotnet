using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Components.Adapters.GoogleBusiness
{
    internal class ValidationHelper
    {
        public static async Task<bool> ValidateRequest(string body, string signature, string partnerKey)
        {
            byte[] b = new HMACSHA512(Encoding.UTF8.GetBytes(partnerKey)).ComputeHash(Encoding.UTF8.GetBytes(body));
            var hash = Convert.ToBase64String(b);
            return string.Equals(hash, signature, StringComparison.InvariantCulture);
        }
    }
}