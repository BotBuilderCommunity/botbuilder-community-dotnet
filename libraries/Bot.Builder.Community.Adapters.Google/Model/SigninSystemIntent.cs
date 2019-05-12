using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Community.Adapters.Google.Model
{
    public class SigninSystemIntent : ISystemIntent
    {
        public SigninSystemIntent()
        {
            intent = "actions.intent.SIGN_IN";
            data = new Data() {type = "type.googleapis.com/google.actions.v2.SignInValueSpec"};
        }

        public string intent { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public string type { get; set; }
    }
}
