using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Components.Middleware.Multilingual.AzureTranslateService;

namespace Bot.Builder.Community.Components.Middleware.Multilingual.Middleware
{
    public partial class MultilingualMiddleware : IMiddleware
    {
        private UserState _userState;
        private readonly IStatePropertyAccessor<string> _languageStateProperty;
        private readonly ITranslateService _translateService;
        
        public MultilingualMiddleware(UserState userState, ITranslateService translateService)
        {
            this._userState = userState;
            this._languageStateProperty = userState.CreateProperty<string>(nameof(_languageStateProperty));
            this._translateService = translateService;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (_translateService.IsMultilingualEnabled)
            {
                var userLanguage = await GetCurrentLanguage(turnContext, cancellationToken);

                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    if (string.IsNullOrEmpty(userLanguage))
                    {
                        userLanguage = await FindLanguage(turnContext);
                    }

                    if (string.CompareOrdinal(userLanguage, _translateService.DefaultLanguageCode) != 0)
                    {
                        turnContext.Activity.Text = await _translateService.TranslateText(userLanguage,
                            _translateService.DefaultLanguageCode, turnContext.Activity.Text);
                    }

                    await _languageStateProperty.SetAsync(turnContext, userLanguage, cancellationToken)
                        .ConfigureAwait(false);

                }


                turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
                {
                    userLanguage = await GetCurrentLanguage(turnContext, cancellationToken);

                    if (string.IsNullOrEmpty(userLanguage) ||
                        string.CompareOrdinal(userLanguage, _translateService.DefaultLanguageCode) == 0)
                    {
                        return await nextSend().ConfigureAwait(false);
                    }

                    var tasks = activities.Where(a => a.Type == ActivityTypes.Message)
                        .Select(activity =>
                            TranslateMessageActivityAsync(activity.AsMessageActivity(), userLanguage)).ToList();

                    if (tasks.Any())
                    {
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                    }

                    return await nextSend();
                });

                turnContext.OnUpdateActivity(async (ctx, activities, nextUpdate) =>
                {
                    if (activities.Type == ActivityTypes.Message)
                    {
                        userLanguage = await GetCurrentLanguage(turnContext, cancellationToken);

                        if (string.IsNullOrEmpty(userLanguage) ||
                            string.CompareOrdinal(userLanguage, _translateService.DefaultLanguageCode) == 0)
                        {
                            return await nextUpdate().ConfigureAwait(false);
                        }

                        await TranslateMessageActivityAsync(activities.AsMessageActivity(), userLanguage);
                    }

                    return await nextUpdate();
                });
            }

            await next(cancellationToken).ConfigureAwait(false);
        }

    }
}
