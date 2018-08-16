using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Middleware.BestMatch
{
    public abstract class BestMatchMiddleware : IMiddleware
    {
        protected Dictionary<BestMatchAttribute, BestMatchHandler> HandlerByBestMatchLists;

        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            if (context.Activity.Type == ActivityTypes.Message)
            {
                await HandleMessage(context, context.Activity.Text, next);
            }
        }

        private async Task HandleMessage(ITurnContext context, string messageText, MiddlewareSet.NextDelegate next)
        {
            if (HandlerByBestMatchLists == null)
            {
                HandlerByBestMatchLists =
                    new Dictionary<BestMatchAttribute, BestMatchHandler>(GetHandlersByBestMatchLists());
            }

            BestMatchHandler handler = null;

            double bestMatchedScore = 0;

            foreach (var handlerByBestMatchList in HandlerByBestMatchLists)
            {
                var match = FindBestMatch(handlerByBestMatchList.Key.BestMatchList,
                    messageText,
                    handlerByBestMatchList.Key.Threshold,
                    handlerByBestMatchList.Key.IgnoreCase,
                    handlerByBestMatchList.Key.IgnoreNonAlphanumericCharacters);

                if (match?.Score > bestMatchedScore)
                {
                    bestMatchedScore = match.Score;
                    handler = handlerByBestMatchList.Value;
                }
            }

            await (handler ?? NoMatchHandler).Invoke(context, messageText, next);
        }

        protected virtual IDictionary<BestMatchAttribute, BestMatchHandler> GetHandlersByBestMatchLists()
        {
            return EnumerateHandlers(this).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static StringMatch FindBestMatch(IEnumerable<string> choices, string utterance, double threshold = 0.5, bool ignoreCase = true, bool ignoreNonAlphanumeric = true)
        {
            StringMatch bestMatch = null;
            var matches = FindAllMatches(choices, utterance, threshold, ignoreCase, ignoreNonAlphanumeric);
            foreach (var match in matches)
            {
                if (bestMatch == null || match.Score > bestMatch.Score)
                {
                    bestMatch = match;
                }
            }
            return bestMatch;
        }

        private static IEnumerable<StringMatch> FindAllMatches(IEnumerable<string> choices, string utterance, double threshold = 0.6, bool ignoreCase = true, bool ignoreNonAlphanumeric = true)
        {
            var matches = new List<StringMatch>();

            var choicesList = choices as IList<string> ?? choices.ToList();

            if (!choicesList.Any())
                return matches;

            var utteranceToCheck = ignoreNonAlphanumeric
                ? Regex.Replace(utterance, @"[^A-Za-z0-9 ]", string.Empty)
                : utterance;

            var tokens = utterance.Split(' ');

            foreach (var choice in choicesList)
            {
                double score = 0;
                var choiceValue = choice.Trim();
                if (ignoreNonAlphanumeric)
                    Regex.Replace(choiceValue, @"[^A-Za-z0-9 ]", string.Empty);

                if (choiceValue.IndexOf(utteranceToCheck, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0)
                {
                    score = (double)decimal.Divide((decimal)utteranceToCheck.Length, (decimal)choiceValue.Length);
                }
                else if (utteranceToCheck.IndexOf(choiceValue) >= 0)
                {
                    score = Math.Min(0.5 + (choiceValue.Length / utteranceToCheck.Length), 0.9);
                }
                else
                {
                    foreach (var token in tokens)
                    {
                        var matched = string.Empty;

                        if (choiceValue.IndexOf(token, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0)
                        {
                            matched += token;
                        }

                        score = (double)decimal.Divide((decimal)matched.Length, (decimal)choiceValue.Length);
                    }
                }

                if (score >= threshold)
                {
                    matches.Add(new StringMatch { Choice = choiceValue, Score = score });
                }
            }

            return matches;
        }

        internal static IEnumerable<KeyValuePair<BestMatchAttribute, BestMatchHandler>> EnumerateHandlers(object dialog)
        {
            var type = dialog.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var method in methods)
            {
                var bestMatchListAttributes = method.GetCustomAttributes<BestMatchAttribute>(inherit: true).ToArray();
                Delegate created = null;
                try
                {
                    created = Delegate.CreateDelegate(typeof(BestMatchHandler), dialog, method, throwOnBindFailure: false);
                }
                catch (ArgumentException)
                {
                    // "Cannot bind to the target method because its signature or security transparency is not compatible with that of the delegate type."
                    // https://github.com/Microsoft/BotBuilder/issues/634
                    // https://github.com/Microsoft/BotBuilder/issues/435
                }

                var bestMatchHandler = (BestMatchHandler)created;
                if (bestMatchHandler != null)
                {
                    foreach (var bestMatchListAttribute in bestMatchListAttributes)
                    {
                        if (bestMatchListAttribute != null && bestMatchListAttributes.Any())
                            yield return new KeyValuePair<BestMatchAttribute, BestMatchHandler>(bestMatchListAttribute, bestMatchHandler);
                    }
                }
            }
        }

        public virtual async Task NoMatchHandler(ITurnContext context, string messageText, MiddlewareSet.NextDelegate next)
        {
            await next();
        }

        internal class StringMatch
        {
            public string Choice { get; set; }
            public double Score { get; set; }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class BestMatchAttribute : Attribute
    {
        public readonly string BestMatchListDelimited;
        public readonly string[] BestMatchList;
        public readonly bool IgnoreCase;
        public readonly bool IgnoreNonAlphanumericCharacters;
        public readonly double Threshold;

        public BestMatchAttribute(string[] bestMatchList, double threshold = 0.5, bool ignoreCase = true, bool ignoreNonAlphaNumericCharacters = true)
        {
            BestMatchList = bestMatchList;
            IgnoreCase = ignoreCase;
            IgnoreNonAlphanumericCharacters = ignoreNonAlphaNumericCharacters;
            Threshold = threshold;
        }

        public BestMatchAttribute(string bestMatchListDelimited, double threshold = 0.5, bool ignoreCase = true, bool ignoreNonAlphaNumericCharacters = true, char listDelimiter = ',')
        {
            BestMatchListDelimited = bestMatchListDelimited;

            if (!string.IsNullOrEmpty(bestMatchListDelimited))
            {
                BestMatchList = StringToListString(bestMatchListDelimited, listDelimiter).ToArray<string>();
            }

            IgnoreCase = ignoreCase;
            IgnoreNonAlphanumericCharacters = ignoreNonAlphaNumericCharacters;
            Threshold = threshold;
        }

        public static IEnumerable<string> StringToListString(string str, char delimiter = ',')
        {
            if (String.IsNullOrEmpty(str))
                yield break;

            foreach (var s in str.Split(delimiter))
            {
                yield return s;
            }
        }
    }

    public delegate Task BestMatchHandler(ITurnContext context, string messageText, MiddlewareSet.NextDelegate next);
}
