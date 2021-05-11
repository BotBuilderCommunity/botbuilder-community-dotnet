namespace Bot.Builder.Community.Components.Handoff.ServiceNow.Models
{
    public interface IServiceNowCredentialsProvider
    {
        string ServiceNowTenant { get; }
        string ServiceNowAuthConnectionName { get; }
        string MsAppId { get; }
    }
}