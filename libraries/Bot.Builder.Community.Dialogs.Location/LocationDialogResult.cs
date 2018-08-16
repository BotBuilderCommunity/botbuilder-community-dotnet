using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Dialogs.Location
{
    public class LocationDialogResult : Dictionary<string, object>
    {
        public Place SelectedLocation
        {
            get { return GetProperty<Place>(nameof(SelectedLocation)); }
            set { this[nameof(SelectedLocation)] = value; }
        }

        public bool HasLocation => SelectedLocation != null;

        protected T GetProperty<T>(string propertyName)
        {
            if (ContainsKey(propertyName))
            {
                return (T)this[propertyName];
            }
            return default(T);
        }
    }
}
