using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Bot.Builder.Community.Dialogs.FormFlow;
using Bot.Builder.Community.Dialogs.FormFlow.Advanced;
using Microsoft.Bot.Schema;

namespace FormFlow_Sample.Dialogs
{
    // from: https://github.com/Microsoft/BotBuilder-V3/blob/master/CSharp/Samples/Microsoft.Bot.Sample.FormFlowAttachmentsBot/ImagesForm.cs

    [Serializable]
    public class MyAwaitableImage : AwaitableAttachment
    {
        // Mandatory: you should have this ctor as it is used by the recognizer
        public MyAwaitableImage(Attachment source) : base(source) { }

        // Mandatory: you should have this serialization ctor as well & call base
        protected MyAwaitableImage(SerializationInfo info, StreamingContext context) : base(info, context) { }

        // Optional: here you can check for content-type for ex 'image/png' or other..
        public override async Task<ValidateResult> ValidateAsync<T>(IField<T> field, T state)
        {
            var result = await base.ValidateAsync(field, state);

            if (result.IsValid)
            {
                var isValidForMe = this.Attachment.ContentType.ToLowerInvariant().Contains("image/png");

                if (!isValidForMe)
                {
                    result.IsValid = false;
                    result.Feedback = $"Hey, dude! Provide a proper 'image/png' attachment, not any file on your computer like '{this.Attachment.Name}'!";
                }
            }

            return result;
        }

        // Optional: here you can provide additional or override custom help text completely..
        public override string ProvideHelp<T>(IField<T> field)
        {
            var help = base.ProvideHelp(field);

            help += $"{Environment.NewLine}- Only 'image/png' can be attached to this field.";

            return help;
        }

        // Optional: here you can define your custom logic to get the attachment data or add custom logic to check it, etc..
        protected override async Task<Stream> ResolveFromSourceAsync(Attachment source)
        {
            var result = await base.ResolveFromSourceAsync(source);

            // You can apply custom logic to result or avoid calling base and resolve it yourself
            // For ex. if you plan to use your instance several times you can return a MemoryStream instead

            return result;
        }
    }

    [Serializable]
    public class ImagesForm
    {
        // Attachment field has no validation - any attachment would work
        public AwaitableAttachment BestImage;

        // Attachment field is optional - validation is done through AttachmentContentTypeValidator usage
        [Optional]
        [AttachmentContentTypeValidator(ContentType = "png")]
        public AwaitableAttachment SecondaryImage;

        // You can use an AwaitableAttachment descendant in order to have your own custom logic
        public IEnumerable<MyAwaitableImage> CustomImages;

        public static IForm<ImagesForm> BuildForm()
        {
            OnCompletionAsyncDelegate<ImagesForm> onFormCompleted = async (context, state) =>
            {
                await context.PostAsync("Here is a summary of the data you submitted:");

                var bestImageSize = await RetrieveAttachmentSizeAsync(state.BestImage);
                await context.PostAsync($"Your first image is '{state.BestImage.Attachment.Name}' - Type: {state.BestImage.Attachment.ContentType} - Size: {bestImageSize} bytes");

                if (state.SecondaryImage != null)
                {
                    var secondaryImageSize = await RetrieveAttachmentSizeAsync(state.SecondaryImage);
                    await context.PostAsync($"Your second image is '{state.SecondaryImage.Attachment.Name}' - Type: {state.SecondaryImage.Attachment.ContentType} - Size: {secondaryImageSize} bytes");
                }
                else
                {
                    await context.PostAsync($"You didn't submit a second image");
                }

                var customImagesTextInfo = string.Empty;
                foreach (var image in state.CustomImages)
                {
                    var imgSize = await RetrieveAttachmentSizeAsync(image);
                    customImagesTextInfo += $"{Environment.NewLine}- Name: '{image.Attachment.Name}' - Type: {image.Attachment.ContentType} - Size: {imgSize} bytes";
                }

                await context.PostAsync($"Here is the info of custom images you submitted: {customImagesTextInfo}");
            };

            // Form localization is done by setting the thread culture
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-us");

            return new FormBuilder<ImagesForm>()
                .Message("Welcome, please submit all required images")
                .OnCompletion(onFormCompleted)
                .Build();
        }

        private static async Task<long> RetrieveAttachmentSizeAsync(AwaitableAttachment attachment)
        {
            var stream = await attachment;
            return stream.Length;
        }
    }
}