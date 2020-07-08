using System.Collections.Generic;
using Bot.Builder.Community.Adapter.ActionsSDK.Core.Model;
using Bot.Builder.Community.Adapter.ActionsSDK.Core.Model.ContentItems;

namespace Bot.Builder.Community.Adapter.ActionsSDK.Core.Helpers
{
    public static class ContentItemFactory
    {
        public static CardContentItem CreateCard(string title, string subtitle, Link button = null, Image image = null,
            ImageFill? imageFill = null)
        {
            var cardContentItem = new CardContentItem()
            {
                Card = new Card()
                {
                    Title = title,
                    Subtitle = subtitle,
                    Image = image,
                    Button = button
                }
            };

            if (imageFill.HasValue)
            {
                cardContentItem.Card.ImageFill = imageFill.Value;
            }

            return cardContentItem;
        }

        public static TableContentItem CreateTable(List<TableColumn> columns, List<TableRow> rows, string title = null,
            string subtitle = null, Link button = null, Image image = null)
        {
            return new TableContentItem()
            {
                Table = new Table()
                {
                    Columns = columns,
                    Rows = rows,
                    Title = title,
                    Subtitle = subtitle,
                    Button = button,
                    Image = image
                }
            };
        }

        public static ListContentItem CreateList(List<ListItem> items, string title = null, string subtitle = null)
        {
            return new ListContentItem()
            {
                Title = title,
                Subtitle = subtitle,
                Items = items
            };
        }

        public static CollectionContentItem CreateCollection(List<CollectionItem> items, string title = null, string subtitle = null, ImageFill? imageFill = null)
        {
            var collectionContentItem = new CollectionContentItem()
            {
                Title = title,
                Subtitle = subtitle,
                Items = items,
            };

            if (imageFill.HasValue)
            {
                collectionContentItem.ImageFill = imageFill.Value;
            }

            return collectionContentItem;
        }

        public static ImageContentItem CreateImage(string url, int width = 0, int height = 0, string alt = null)
        {
            return new ImageContentItem()
            {
                Image = new Image()
                {
                    Url = url,
                    Width = width,
                    Height = height,
                    Alt = alt
                }
            };
        }

        public static MediaContentItem CreateMedia(List<MediaObject> mediaObjects, MediaType mediaType, List<OptionalMediaControl> optionalControls = null, string startOffset = null)
        {
            return new MediaContentItem()
            {
                Media = new Media()
                {
                    MediaObjects = mediaObjects,
                    MediaType = mediaType,
                    OptionsMediaControls = optionalControls,
                    StartOffset = startOffset
                }
            };
        }
    }
}
