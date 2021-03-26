namespace Bot.Builder.Community.Components.Handoff.ServiceNow.Models
{
    public interface IServiceNowCredentialsProvider
    {
        string ServiceNowTenant { get; }
        string UserName { get; }
        string Password { get; }        
        string MsAppId { get; }
    }
}