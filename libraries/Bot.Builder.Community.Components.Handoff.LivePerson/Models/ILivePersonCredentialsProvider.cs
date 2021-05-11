namespace Bot.Builder.Community.Components.Handoff.LivePerson.Models
{
    public interface ILivePersonCredentialsProvider
    {
        string LpAccount { get; }
        string LpAppId { get; }
        string LpAppSecret { get; }
        string MsAppId { get; }
    }
}