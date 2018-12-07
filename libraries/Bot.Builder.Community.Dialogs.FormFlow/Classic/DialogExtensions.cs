using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Dialogs.FormFlow
{
    public static class DialogExtensions
    {
		public static IMessageActivity MakeMessage(this DialogContext dialogContext)
		{
			return dialogContext.Context.Activity.CreateReply();
		}

		public static async Task PostAsync(this DialogContext dialogContext, IMessageActivity activity)
		{
			await dialogContext.Context.SendActivityAsync(activity);
		}

		public static async Task PostAsync(this DialogContext dialogContext, string text)
		{
			await dialogContext.Context.SendActivityAsync(text);
		}
		public static async Task Done(this DialogContext dialogContext, object result = null)
		{
			await dialogContext.EndDialogAsync(result);
		}
	}
}
