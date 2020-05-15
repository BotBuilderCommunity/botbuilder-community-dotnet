using System;
using System.Collections.Generic;
using System.Text;
using Bot.Builder.Community.Adapters.Google.Core.Model.Response;
using Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents;

namespace Bot.Builder.Community.Adapters.Google.Core.Helpers
{
    public static class GoogleCardFactory
    {
        public static BasicCard CreateBasicCard(string title, string subtitle, string formattedText)
        {
            return new BasicCard()
            {
                Content = new BasicCardContent()
                {

                    Title = title,
                    Subtitle = subtitle,
                    FormattedText = formattedText
                }
            };
        }

        public static TableCard CreateTableCard(List<ColumnProperties> columns, List<Row> rows, string title = null,
            string subtitle = null, List<Button> buttons = null, Image image = null)
        {
            return new TableCard()
            {
                Content = new TableCardContent()
                {
                    ColumnProperties = columns,
                    Rows = rows,
                    Title = title,
                    Subtitle = subtitle,
                    Buttons = buttons,
                    Image = image
                }
            };
        }
    }
}
