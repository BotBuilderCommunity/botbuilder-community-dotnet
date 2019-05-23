using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter;

namespace Bot.Builder.Community.Adapters.Twitter.Webhooks.Models
{

    /// <summary>
    /// Wrapper to wrap any result for better errors understanding.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Result<T>
    {
        public Result()
        {
            Success = false;
        }
        public Result(T data)
        {
            Data = data;
            Success = true;
        }

        public Result(TwitterError err)
        {
            Error = err;
            Success = false;

        }

        public T Data { get; private set; }

        public TwitterError Error { get; private set; }

        public bool Success { get; private set; }

    }
}
