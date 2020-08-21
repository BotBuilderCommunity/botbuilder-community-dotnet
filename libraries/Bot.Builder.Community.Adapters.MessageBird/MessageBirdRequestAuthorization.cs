using MessageBird;
using MessageBird.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Adapters.MessageBird
{
    public class MessageBirdRequestAuthorization
    {
        public bool Verify(string _messageBirdSignature, string _signingKey, string _timeStampt, string _requestBody)
        {
            byte[] _requestBodyByte = Encoding.ASCII.GetBytes(_requestBody);

            var requestSigner = new RequestSigner(Encoding.ASCII.GetBytes(_signingKey));
            // @param expectedSignature Signature from the MessageBird-Signature header in its original base64 encoded state.
            //const string expectedSignature = _messageBirdSignature;
            var request = new Request(_timeStampt, "", _requestBodyByte);
            return requestSigner.IsMatch(_messageBirdSignature, request);
            //return true;
        }
    }
}
