namespace Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents
{
    public class SigninIntent : SystemIntent
    {
        public SigninIntent()
        {
            Intent = "actions.intent.SIGN_IN";
        }

        public IntentInputValueData InputValueData => new IntentInputValueData {Type = "type.googleapis.com/google.actions.v2.SignInValueSpec"};
    }
}