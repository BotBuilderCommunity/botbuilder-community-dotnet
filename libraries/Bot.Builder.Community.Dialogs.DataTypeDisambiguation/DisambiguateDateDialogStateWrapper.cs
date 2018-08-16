using System;
using System.Collections.Generic;

namespace Bot.Builder.Community.Dialogs.DataTypeDisambiguation
{
    // convenience helper to get/set dialog state
    internal class DisambiguateDateDialogStateWrapper
    {
        public DisambiguateDateDialogStateWrapper(IDictionary<string,object> state)
        {
            State = state;
        }

        public IDictionary<string, object> State { get; }

        public RecognizerExtensions.ResolutionList Resolutions {
            get
            {
                return State["recognizedDateTime"] as RecognizerExtensions.ResolutionList;
            }
            set
            {
                State["recognizedDateTime"] = value;
            }
        }

        public DateTime Date
        {
            get
            {
                return (DateTime)State["date"];
            }
            set
            {
                State["date"] = value;
            }
        }
    }
}
