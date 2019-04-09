using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cortana_Assistant_Alexa_Sample.Helpers
{
    internal class Messages
    {
        private static List<string> rateMessages;
        private static List<string> rateCaratMessages;
        private static List<string> noneMessages;

        public static string GetNoneMessages()
        {
            noneMessages = new List<string>();
            noneMessages.Add("Sorry, I'm only trained for gold rates of the day.");
            noneMessages.Add("Sorry, I did not get what you mean by that. I am only good at telling gold rates of the day.");
            noneMessages.Add("I'm sorry, can you please be specific just about the gold rates?");
            Random random = new Random(DateTime.Now.Second);
            return noneMessages[random.Next(noneMessages.Count)];
        }

        public static string GetRateMessages()
        {
            rateMessages = new List<string>();
            rateMessages.Add("The gold rate of today is {0} dollars for 24 carat and {1} dollars for 22 carat.");
            rateMessages.Add("For 24 carat, it's {0} dollars and for 22 carat, it's {1} dollars.");
            rateMessages.Add("Today's rates are, {0} dollars for 24 carat and {1} dollars for 22 carat.");
            Random random = new Random(DateTime.Now.Second);
            return rateMessages[random.Next(rateMessages.Count)];
        }

        public static string GetRateCaratMessages()
        {
           rateCaratMessages = new List<string>();
           rateCaratMessages.Add("Today's gold rate for {0} carat is {1} dollars.");
           rateCaratMessages.Add("For {0} carat, it's {1} dollars today.");
           Random random = new Random(DateTime.Now.Second);
           return rateCaratMessages[random.Next(rateCaratMessages.Count)];
        }
    }
}
