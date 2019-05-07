using System.Collections.Generic;

namespace Bot.Builder.Community.Dialogs.Luis
{
    public interface IResolutionParser
    {
        bool TryParse(IDictionary<string, object> properties, out Resolution resolution);
    }
}
