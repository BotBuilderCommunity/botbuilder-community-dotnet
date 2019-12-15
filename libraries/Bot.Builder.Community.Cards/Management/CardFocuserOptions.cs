namespace Bot.Builder.Community.Cards.Management
{
    public class CardFocuserOptions
    {
        public bool AutoApplyId { get; set; } = true;

        public bool AutoFocus { get; set; } = true;

        public bool AutoUnfocus { get; set; } = true;

        public IdType IdType { get; set; } = IdType.Batch;
    }
}