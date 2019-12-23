namespace Bot.Builder.Community.Cards.Management
{
    public class CardActivity
    {
        public CardActivity(string activityId, string cardName = null)
        {
            ActivityId = activityId;
            CardName = cardName;
        }

        public string ActivityId { get; }

        public string CardName { get; }
    }
}