using System.Collections.Generic;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Model.ContentItems
{
    public class Table
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public Image Image { get; set; }
        public List<TableColumn> Columns { get; set; }
        public List<TableRow> Rows { get; set; }
        public Link Button { get; set; }
    }
}
