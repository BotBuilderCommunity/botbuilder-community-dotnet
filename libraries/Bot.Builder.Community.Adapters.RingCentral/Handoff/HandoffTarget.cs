namespace Bot.Builder.Community.Adapters.RingCentral.Handoff
{
    /// <summary>
    /// Specifies possible targets to handoff a conversation to.
    /// </summary>
    public enum HandoffTarget
    {
        /// <summary>
        /// No handoff.
        /// </summary>
        None,
        
        /// <summary>
        /// A human should give responses.
        /// </summary>
        Agent,
        
        /// <summary>
        /// Bot should give replies.
        /// </summary>
        Bot
    }
}
