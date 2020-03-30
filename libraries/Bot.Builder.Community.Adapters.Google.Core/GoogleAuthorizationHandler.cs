using System;
using Microsoft.Extensions.Logging;

namespace Bot.Builder.Community.Adapters.Google.Core
{
    /// <summary>
    /// Google Authorization Handler.
    /// </summary>
    public class GoogleAuthorizationHandler
    {
        private readonly ILogger _logger;

        public GoogleAuthorizationHandler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Verify the action request is from the designated actionId.
        /// </summary>
        /// <param name="actionId">Alexa Skill Id.</param>
        /// <returns>True if the request is for this skill.</returns>
        public virtual bool ValidateActionId(string actionId)
        {
            throw new NotImplementedException();
        }
    }
}
