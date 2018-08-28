using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;

namespace Dialog_Sample
{
    public class MyEchoBotAccessors
    {
        public static string DialogStateName = $"{nameof(MyEchoBotAccessors)}.DialogState";
        
        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }

    }
}