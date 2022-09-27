using Polly;
using Polly.Retry;

namespace Foxminded.Task11.Policies
{
    public class ClientPolicy
    {
        public AsyncRetryPolicy<HttpResponseMessage> LinearHttpRetry { get; }

        public ClientPolicy()
        {
            LinearHttpRetry = Policy.HandleResult<HttpResponseMessage>(
                res => !res.IsSuccessStatusCode).
                WaitAndRetryAsync(5, retryAttempts => TimeSpan.FromSeconds(3));
        }
    }
}
