using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Bot.Builder.Community.Dialogs.Luis
{
    /// <summary>
    /// An exception for invalid intent handlers.
    /// </summary>
    [Serializable]
    public sealed class InvalidIntentHandlerException : InvalidOperationException
    {
#pragma warning disable SA1401 // Fields must be private
        public readonly MethodInfo Method;
#pragma warning restore SA1401 // Fields must be private

        public InvalidIntentHandlerException(string message, MethodInfo method)
            : base(message)
        {
            SetField.NotNull(out this.Method, nameof(method), method);
        }

        private InvalidIntentHandlerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
