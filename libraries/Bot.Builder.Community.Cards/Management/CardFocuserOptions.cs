namespace Bot.Builder.Community.Cards.Management
{
    public class CardFocuserOptions
    {
        public bool AutoApplyId { get; set; } = true;

        public bool AutoFocus { get; set; } = true;

        public bool AutoUnfocusOnAction { get; set; } = true;

        public IdType IdType { get; set; } = IdType.Batch;
        public bool AutoUnfocusOnSend { get; internal set; }
    }
}