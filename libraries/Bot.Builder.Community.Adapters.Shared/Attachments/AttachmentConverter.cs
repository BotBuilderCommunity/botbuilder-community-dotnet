using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Bot.Builder.Community.Adapters.Shared.Attachments
{
    public class AttachmentConverter
    {
        /// <summary>
        /// These are the default converters. All adapters should use these converters.
        /// </summary>
        private static readonly IReadOnlyList<IAttachmentConverter> _defaultAttachmentConverters = new[]
        {
            new CardAttachmentConverter()
        };

        private readonly List<IAttachmentConverter> _converters = new List<IAttachmentConverter>();

        public AttachmentConverter(bool useDefaults = true)
        {
            _converters = useDefaults ? new List<IAttachmentConverter>(_defaultAttachmentConverters) : new List<IAttachmentConverter>();
        }

        public AttachmentConverter(IAttachmentConverter converter, bool useDefaults = true) : this(useDefaults)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            _converters.Add(converter);
        }

        public IReadOnlyList<IAttachmentConverter> Converters { get { return _converters; } }

        public void ConvertAttachments(Activity activity)
        {
            if (activity == null || activity.Attachments == null)
            {
                return;
            }

            foreach (var attachment in activity.Attachments)
            {
                foreach (var converter in Converters)
                {
                    if (converter.ConvertAttachmentContent(attachment))
                        break;
                }
            }
        }
    }
}
