using System;
using Bot.Builder.Community.Adapters.ActionsSDK.Core.Model.ContentItems;
using Bot.Builder.Community.Adapters.Shared;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.ActionsSDK.Core.Attachments
{
    public static class AttachmentHelper
    {
        /// <summary>
        /// Convert all Actions SDK specific attachments to their correct type.
        /// </summary>
        /// <param name="activity"></param>
        public static void ConvertAttachmentContent(this Activity activity)
        {
            if (activity == null || activity.Attachments == null)
            {
                return;
            }

            foreach (var attachment in activity.Attachments)
            {
                switch (attachment.ContentType)
                {
                    case HeroCard.ContentType:
                        SharedAttachmentHelper.Convert<HeroCard>(attachment);
                        break;
                    case SigninCard.ContentType:
                        SharedAttachmentHelper.Convert<SigninCard>(attachment);
                        break;
                    case ActionsSdkAttachmentContentTypes.Card:
                        SharedAttachmentHelper.Convert<CardContentItem>(attachment);
                        break;
                    case ActionsSdkAttachmentContentTypes.Table:
                        SharedAttachmentHelper.Convert<TableContentItem>(attachment);
                        break;
                    case ActionsSdkAttachmentContentTypes.Media:
                        SharedAttachmentHelper.Convert<MediaContentItem>(attachment);
                        break;
                    case ActionsSdkAttachmentContentTypes.Collection:
                        SharedAttachmentHelper.Convert<CollectionContentItem>(attachment);
                        break;
                    case ActionsSdkAttachmentContentTypes.List:
                        SharedAttachmentHelper.Convert<ListContentItem>(attachment);
                        break;
                }
            }
        }
    }
}
