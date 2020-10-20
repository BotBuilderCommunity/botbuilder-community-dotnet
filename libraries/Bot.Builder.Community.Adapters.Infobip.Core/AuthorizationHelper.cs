using System;
using System.Security.Cryptography;
using System.Text;

namespace Bot.Builder.Community.Adapters.Infobip.Core
{
    public class AuthorizationHelper
    {
        public bool VerifySignature(string signatureValue, string payload, string infobipAppSecret)
        {
            if (string.IsNullOrEmpty(signatureValue))
                return false;

            var expectedSignatureValue = signatureValue.ToUpperInvariant();

            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(infobipAppSecret)))
            {
                hmac.Initialize();
                var hashArray = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var hash = $"SHA1={BitConverter.ToString(hashArray).Replace("-", string.Empty)}";

                return expectedSignatureValue == hash;
            }
        }
    }
}
