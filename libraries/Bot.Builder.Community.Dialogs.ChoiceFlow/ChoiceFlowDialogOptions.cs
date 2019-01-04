using System.Collections.Generic;

namespace Bot.Builder.Community.Dialogs.ChoiceFlow
{
    public class ChoiceFlowDialogOptions
    {
        public List<ChoiceFlowItem> ChoiceFlowItems { get; set; }
        public int SelectedChoiceFlowItem { get; set; }

        public ChoiceFlowDialogOptions()
        {
            SelectedChoiceFlowItem = 0;
        }
    }
}
