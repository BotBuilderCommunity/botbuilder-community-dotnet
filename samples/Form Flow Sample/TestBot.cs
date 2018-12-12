using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Bot.Builder.Community.Dialogs.FormFlow;
using Microsoft.Bot.Schema;
using FormFlow_Sample.Dialogs;
using Microsoft.Bot.Builder;

namespace FormFlow_Sample
{
	public class TestBot : IBot
	{
		private DialogSet _dialogs;
		private SemaphoreSlim _semaphore;

		public TestBot(TestBotAccessors accessors)
		{
			// create the DialogSet from accessor
			_dialogs = new DialogSet(accessors.ConversationDialogState);

			// a semaphore to serialize access to the bot state
			_semaphore = accessors.SemaphoreSlim;

			// add the various named dialogs that can be used
			_dialogs.Add(new MenuDialog());
			_dialogs.Add(FormDialog.FromForm(Order.BuildOrderForm));
            _dialogs.Add(FormDialog.FromForm(SandwichOrder.BuildForm));
			_dialogs.Add(FormDialog.FromForm(BuilderSandwich.BuildForm));
            _dialogs.Add(FormDialog.FromForm(()=>PizzaOrder.BuildForm(localize: true)));
            _dialogs.Add(FormDialog.FromForm(ScheduleCallbackDialog.BuildForm));
            _dialogs.Add(FormDialog.FromForm(ImagesForm.BuildForm));
            _dialogs.Add(new HotelsDialog()); //<--loads a FormFlow dialog and does processing with the results
		}

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            var test = new global::System.Resources.ResourceManager("Form Flow Sample.Resource.DynamicPizza", typeof(Resource.DynamicPizza).Assembly);

            //turnContext.Activity.Locale = Microsoft.Recognizers.Text.Culture.English;
            // We only want to pump one activity at a time through the state.
            // Note the state is shared across all instances of this IBot class so we
            // create the semaphore globally with the accessors.
            try
            {
                await _semaphore.WaitAsync();

                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    // run the DialogSet - let the framework identify the current state of the dialog from 
                    // the dialog stack and figure out what (if any) is the active dialog
                    var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                    // HasActive = true if there is an active dialog on the dialogstack
                    // HasResults = true if the dialog just completed and the final  result can be retrived
                    // if both are false this indicates a new dialog needs to start
                    // an additional check for Responded stops a new waterfall from being automatically started over
                    if ((results.Status == DialogTurnStatus.Cancelled || results.Status == DialogTurnStatus.Empty)
                          || (results.Status == DialogTurnStatus.Complete && dialogContext.ActiveDialog == null))
                    {
                        var text = turnContext.Activity.Text;
                        var foundDialog = _dialogs.Find(text);
                        if (foundDialog != null)
                        {
                            await dialogContext.BeginDialogAsync(foundDialog.Id, null, cancellationToken);
                        }
                        else
                        {
                            await dialogContext.BeginDialogAsync(typeof(MenuDialog).Name, null, cancellationToken);
                        }
                    }
                }
                else if (turnContext.Activity.Type == ActivityTypes.Event && turnContext.Activity.Name == "requestMenuDialog")
                {
                    await dialogContext.BeginDialogAsync(typeof(MenuDialog).Name, null, cancellationToken);
                }
            }
            catch (FormCanceledException ex)
            {
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dialogContext.Context.SendActivityAsync("Form Cancelled");
                await dialogContext.BeginDialogAsync(typeof(MenuDialog).Name, null, cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }

        }
	}
}
