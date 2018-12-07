using Bot.Builder.Community.Dialogs.FormFlow.Resource;
using Microsoft.Bot.Schema;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Dialogs.FormFlow.Advanced
{
    /// <summary>
    /// Abstract base class used by all attributes that use \ref patterns.
    /// </summary>
    public abstract class TemplateBaseAttribute : FormFlowAttribute
    {
        private static Random _generator = new Random();

        /// <summary>
        /// When processing choices {||} in a \ref patterns string, provide a choice for the default value if present.
        /// </summary>
        public BoolDefault AllowDefault { get; set; }

        /// <summary>  
        /// Control case when showing choices in {||} references in a \ref patterns string. 
        /// </summary>
        public CaseNormalization ChoiceCase { get; set; }

        /// <summary>
        /// Format string used for presenting each choice when showing {||} choices in a \ref patterns string.
        /// </summary>
        /// <remarks>The choice format is passed two arguments, {0} is the number of the choice and {1} is the field name.</remarks>
        public string ChoiceFormat { get; set; }

        /// <summary>   
        /// When constructing inline lists of choices using {||} in a \ref patterns string, the string used before the last choice. 
        /// </summary>
        public string ChoiceLastSeparator { get; set; }

        /// <summary>  
        /// When constructing inline choice lists for {||} in a \ref patterns string controls whether to include parentheses around choices. 
        /// </summary>
        public BoolDefault ChoiceParens { get; set; }

        /// <summary>
        /// When constructing inline lists using {||} in a \ref patterns string, the string used between all choices except the last. 
        /// </summary>
        public string ChoiceSeparator { get; set; }

        /// <summary>
        /// How to display choices {||} when processed in a \ref patterns string.
        /// </summary>
        public ChoiceStyleOptions ChoiceStyle { get; set; }

        /// <summary>
        /// Control what kind of feedback the user gets after each input.
        /// </summary>
        public FeedbackOptions Feedback { get; set; }

        /// <summary>
        /// Control case when showing {&amp;} field name references in a \ref patterns string.
        /// </summary>
        public CaseNormalization FieldCase { get; set; }

        /// <summary>
        /// When constructing lists using {[]} in a \ref patterns string, the string used before the last value in the list.
        /// </summary>
        public string LastSeparator { get; set; }

        /// <summary>
        /// When constructing lists using {[]} in a \ref patterns string, the string used between all values except the last.
        /// </summary>
        public string Separator { get; set; }

        /// <summary>
        /// Control case when showing {} value references in a \ref patterns string.
        /// </summary>
        public CaseNormalization ValueCase { get; set; }

        internal bool AllowNumbers
        {
            get
            {
                // You can match on numbers only if they are included in Choices and choices are shown
                return ChoiceFormat.Contains("{0}") && Patterns.Any((pattern) => pattern.Contains("{||}"));
            }
        }

        /// <summary>
        /// The pattern to use when generating a string using <see cref="Advanced.IPrompt{T}"/>.
        /// </summary>
        /// <remarks>If multiple patterns were specified, then each call to this function will return a random pattern.</remarks>
        /// <returns>Pattern to use.</returns>
        public string Pattern()
        {
            var choice = 0;
            if (Patterns.Length > 1)
            {
                lock (_generator)
                {
                    choice = _generator.Next(Patterns.Length);
                }
            }
            return Patterns[choice];
        }

        /// <summary>
        /// All possible templates.
        /// </summary>
        /// <returns>The possible templates.</returns>
        public string[] Patterns { get; set; }

        /// <summary>
        /// Any default values in this template will be overridden by the supplied <paramref name="defaultTemplate"/>.
        /// </summary>
        /// <param name="defaultTemplate">Default template to use to override default values.</param>
        public void ApplyDefaults(TemplateBaseAttribute defaultTemplate)
        {
            if (AllowDefault == BoolDefault.Default) AllowDefault = defaultTemplate.AllowDefault;
            if (ChoiceCase == CaseNormalization.Default) ChoiceCase = defaultTemplate.ChoiceCase;
            if (ChoiceFormat == null) ChoiceFormat = defaultTemplate.ChoiceFormat;
            if (ChoiceLastSeparator == null) ChoiceLastSeparator = defaultTemplate.ChoiceLastSeparator;
            if (ChoiceParens == BoolDefault.Default) ChoiceParens = defaultTemplate.ChoiceParens;
            if (ChoiceSeparator == null) ChoiceSeparator = defaultTemplate.ChoiceSeparator;
            if (ChoiceStyle == ChoiceStyleOptions.Default) ChoiceStyle = defaultTemplate.ChoiceStyle;
            if (FieldCase == CaseNormalization.Default) FieldCase = defaultTemplate.FieldCase;
            if (Feedback == FeedbackOptions.Default) Feedback = defaultTemplate.Feedback;
            if (LastSeparator == null) LastSeparator = defaultTemplate.LastSeparator;
            if (Separator == null) Separator = defaultTemplate.Separator;
            if (ValueCase == CaseNormalization.Default) ValueCase = defaultTemplate.ValueCase;
        }

        /// <summary>
        /// Initialize with multiple patterns that will be chosen from randomly.
        /// </summary>
        /// <param name="patterns">Possible patterns.</param>
        public TemplateBaseAttribute(params string[] patterns)
        {
            Patterns = patterns;
            Initialize();
        }

        /// <summary>
        /// Initialize from another template.
        /// </summary>
        /// <param name="other">The template to copy from.</param>
        public TemplateBaseAttribute(TemplateBaseAttribute other)
        {
            Patterns = other.Patterns;
            AllowDefault = other.AllowDefault;
            ChoiceCase = other.ChoiceCase;
            ChoiceFormat = other.ChoiceFormat;
            ChoiceLastSeparator = other.ChoiceLastSeparator;
            ChoiceParens = other.ChoiceParens;
            ChoiceSeparator = other.ChoiceSeparator;
            ChoiceStyle = other.ChoiceStyle;
            FieldCase = other.FieldCase;
            Feedback = other.Feedback;
            LastSeparator = other.LastSeparator;
            Separator = other.Separator;
            ValueCase = other.ValueCase;
        }

        private void Initialize()
        {
            AllowDefault = BoolDefault.Default;
            ChoiceCase = CaseNormalization.Default;
            ChoiceFormat = null;
            ChoiceLastSeparator = null;
            ChoiceParens = BoolDefault.Default;
            ChoiceSeparator = null;
            ChoiceStyle = ChoiceStyleOptions.Default;
            FieldCase = CaseNormalization.Default;
            Feedback = FeedbackOptions.Default;
            LastSeparator = null;
            Separator = null;
            ValueCase = CaseNormalization.Default;
        }
    }

    /// <summary>
    /// Abstract base class used for attachment validation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public abstract class AttachmentValidatorAttribute : Attribute
    {
        public string ErrorMessage { get; set; }

        public FormConfiguration Configuration { get; internal set; }

        public abstract Task<bool> IsValidAsync(Attachment attachment, out string errorMessage);

        public abstract string ProvideHelp();
    }

    /// <summary>
    /// Attachment content-type validator attribute.
    /// </summary>
    public class AttachmentContentTypeValidatorAttribute : AttachmentValidatorAttribute
    {
        public AttachmentContentTypeValidatorAttribute() { }

        public string ContentType { get; set; }

        public override Task<bool> IsValidAsync(Attachment attachment, out string errorMessage)
        {
            errorMessage = default(string);

            string[] contentTypes = GetAllowedTypes(this.ContentType);

            var result = contentTypes != null && contentTypes.Any() ? contentTypes.FirstOrDefault(t => attachment.ContentType.ToLowerInvariant().Contains(t)) != null
                                                                              : attachment.ContentType.ToLowerInvariant().Contains(this.ContentType.ToLowerInvariant());

            if (!result)
            {
                var template = this.Configuration.Template(TemplateUsage.AttachmentContentTypeValidatorError);

                errorMessage = !string.IsNullOrWhiteSpace(this.ErrorMessage)
                    ? this.ErrorMessage
                    : string.Format(
                        template.Pattern(),
                        attachment.Name,
                        contentTypes != null ? GetAllowedTypesString(contentTypes) : string.IsNullOrWhiteSpace(this.ContentType) ? string.Empty : $"'{this.ContentType.ToLowerInvariant()}'");
            }

            return Task.FromResult(result);
        }

        public override string ProvideHelp()
        {
            var template = this.Configuration.Template(TemplateUsage.AttachmentContentTypeValidatorHelp);

            string[] contentTypes = GetAllowedTypes(this.ContentType);

            var contentType = contentTypes != null ? GetAllowedTypesString(contentTypes) : string.IsNullOrWhiteSpace(this.ContentType) ? string.Empty : $"'{this.ContentType.ToLowerInvariant()}'";

            return string.Format(template.Pattern(), contentType);
        }

        private string GetAllowedTypesString(string[] contentTypes)
        {
            return string.Join($" {Resources.DefaultContentTypesSeparator} ", contentTypes.Select(t => t.ToLowerInvariant()).Select(t => $"'{t}'"));
        }

        private string[] GetAllowedTypes(string contentTypes)
        {
            if (this.ContentType.Contains("|"))
            {
                return contentTypes.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return null;
        }
    }
}
