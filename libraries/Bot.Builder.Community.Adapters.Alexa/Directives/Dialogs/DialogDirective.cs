using System;
namespace Bot.Builder.Community.Adapters.Alexa.Directives.Dialogs
{
    public abstract class DialogDirective : IAlexaDirective
    {
        public DialogDirective(string intent)
        {
            UpdatedIntent = new AlexaIntent();
            UpdatedIntent.Name = intent;
        }

        public DialogDirective(string intent, AlexaConfirmationState confirmationStatus)
        {
            UpdatedIntent = new AlexaIntent();
            UpdatedIntent.Name = intent;
            UpdatedIntent.ConfirmationStatus = confirmationStatus.ToString();
        }

        public AlexaIntent UpdatedIntent;

        /// <summary>
        /// Sets a slot name and value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="confirmationStatus"></param>
        /// <param name="value"></param>
        public void SetSlot(string name, string value, AlexaConfirmationState confirmationStatus)
        {
            // ensure we have slots
            if (UpdatedIntent.Slots == null) UpdatedIntent.Slots = new System.Collections.Generic.Dictionary<string, AlexaSlot>();

            // get the slot
            AlexaSlot slot;
            if (UpdatedIntent.Slots.ContainsKey(name))
                slot = UpdatedIntent.Slots[name];
            else
            {
                slot = new AlexaSlot();
                UpdatedIntent.Slots.Add(name, slot);
            }

            // now set the slot
            slot.Name = name;
            slot.ConfirmationStatus = confirmationStatus;
            slot.Value = value;
        }
    }
}
