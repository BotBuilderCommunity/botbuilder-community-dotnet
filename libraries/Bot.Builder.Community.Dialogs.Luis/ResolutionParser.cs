using System.Collections.Generic;

namespace Bot.Builder.Community.Dialogs.Luis
{
    public sealed class ResolutionParser : IResolutionParser
    {
        bool IResolutionParser.TryParse(IDictionary<string, object> properties, out Resolution resolution)
        {
            if (properties != null)
            {
                object value;
                if (properties.TryGetValue("resolution_type", out value) && value is string)
                {
                    switch (value as string)
                    {
                        case "builtin.datetime.date":
                            if (properties.TryGetValue("date", out value) && value is string)
                            {
                                BuiltIn.DateTime.DateTimeResolution dateTime;
                                if (BuiltIn.DateTime.DateTimeResolution.TryParse(value as string, out dateTime))
                                {
                                    resolution = dateTime;
                                    return true;
                                }
                            }

                            break;
                        case "builtin.datetime.time":
                        case "builtin.datetime.set":
                            if (properties.TryGetValue("time", out value) && value is string)
                            {
                                BuiltIn.DateTime.DateTimeResolution dateTime;
                                if (BuiltIn.DateTime.DateTimeResolution.TryParse(value as string, out dateTime))
                                {
                                    resolution = dateTime;
                                    return true;
                                }
                            }

                            break;
                    }
                }
            }

            resolution = null;
            return false;
        }
    }
}
