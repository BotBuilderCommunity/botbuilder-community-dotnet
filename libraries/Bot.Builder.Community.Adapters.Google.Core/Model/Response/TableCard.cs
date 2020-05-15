using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Response
{
    public class TableCard : ResponseItem
    {
        [JsonProperty(PropertyName = "tableCard")]
        public TableCardContent Content { get; set; }
    }

    public class TableCardContent
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public Image Image { get; set; }
        public List<ColumnProperties> ColumnProperties { get; set; }
        public List<Row> Rows { get; set; }
        public List<Button> Buttons { get; set; }
    }

    public class ColumnProperties
    {
        public string Header { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
    }

    public class Row
    {
        public List<Cell> Cells { get; set; }
    }

    public class Cell
    {
        public string Text { get; set; }
    }

    public enum HorizontalAlignment
    {
        LEADING,
        CENTER,
        TRAILING
    }
}