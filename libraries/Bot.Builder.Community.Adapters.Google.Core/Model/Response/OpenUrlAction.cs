namespace Bot.Builder.Community.Adapters.Google.Core.Model.Response
{
    public class OpenUrlAction
    {
        public string Url { get; set; }
        public AndroidApp AndroidApp { get; set; }
        public UrlTypeHint UrlTypeHint { get; set; }
    }

    public enum UrlTypeHint
    {
        URL_TYPE_HINT_UNSPECIFIED,
        AMP_CONTENT
    }
}