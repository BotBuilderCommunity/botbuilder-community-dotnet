using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Models.Twitter;
using Bot.Builder.Community.Adapters.Twitter.Webhooks.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bot.Builder.Community.Adapters.Twitter.Hosting
{        
    /// <inheritdoc />
    /// <summary>
    /// Webhook Hosted Service
    /// </summary>
    public class WebhookHostedService : IHostedService
    {
        private readonly ILogger<WebhookHostedService> _logger;
        private readonly TwitterOptions _options;
        private readonly WebhooksPremiumManager _webhooksManager;
        private readonly SubscriptionsManager _subscriptionsManager;

        public WebhookHostedService(
            IApplicationLifetime applicationLifetime,
            IOptions<TwitterOptions> options,
            ILogger<WebhookHostedService> logger)
        {
            _logger = logger;
            _options = options.Value;
            _webhooksManager = new WebhooksPremiumManager(_options);
            _subscriptionsManager = new SubscriptionsManager(_options);

            // Initialize logic after host has started, to ensure WebhookMiddleware
            // is available for webhook registration
            applicationLifetime.ApplicationStarted.Register(InitializeWebhookAsync);
        }

        private async void InitializeWebhookAsync()
        {
            if (_options.Tier != TwitterAccountApi.PremiumFree)
            {
                throw new NotSupportedException($"{_options.Tier} tier not yet supported");
            }

            var webhooks = await _webhooksManager.GetRegisteredWebhooks();
            if (webhooks.Success)
            {
                if (webhooks.Data.Environments.FirstOrDefault(x => x.Name == _options.Environment) is EnvironmentRegistration environmentRegistration
                    && environmentRegistration.Webhooks.FirstOrDefault() is WebhookRegistration webhookRegistration)
                {
                    if (webhookRegistration.RegisteredUrl == _options.WebhookUri)
                    {
                        if (webhookRegistration.IsValid)
                        {
                            // Webhook registered and valid.
                            _logger.LogInformation("Found valid webhook {WebHook} for environment {Environment}", 
                                _options.WebhookUri, _options.Environment);
                        }
                        else
                        {
                            _logger.LogWarning("Found invalid webhook {WebHook} for environment {Environment}. Attempting to update....", 
                                _options.WebhookUri, _options.Environment);
                            // Call update webhook to initiate CRC 
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Found webhook '{webhookRegistration.RegisteredUrl}', but configured uri is '{_options.WebhookUri}' " +
                                         $"for environment '{_options.Environment}'. Attempting to update...");

                        var removeResult = await _webhooksManager.UnregisterWebhook(webhookRegistration.Id, _options.Environment);

                        if (!removeResult.Success)
                        {
                            _logger.LogError("Failed to remove old webhook.");
                            return;
                        }

                        // Webhook Url is different than current one. Register new webhook.
                        // This will override the webhook in PremiumFree tier, as only one webhook per environment is allowed
                        var result = await _webhooksManager.RegisterWebhook(_options.WebhookUri, _options.Environment);
                        if (result.Success)
                        {
                            _logger.LogInformation($"Webhook registration initiated");
                        }
                        else
                        {
                            _logger.LogError($"Webhook registration error: {string.Join(", ", result.Error.Errors.Select(x => x.Message))}");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"Webhook not found. Registering [{_options.WebhookUri}] for [{_options.Environment}]");

                    await _webhooksManager.RegisterWebhook(_options.WebhookUri, _options.Environment);
                }

                // Check subscription
                var checkSubResult = await _subscriptionsManager.CheckSubscription(_options.Environment);
                if (checkSubResult.Success)
                {
                    if (checkSubResult.Data)
                    {
                        _logger.LogInformation("Found valid subscription");
                    }
                    else
                    {
                        var subResult = await _subscriptionsManager.Subscribe(_options.Environment);
                        if (subResult.Success)
                        {
                            _logger.LogInformation("Subscription registration completed");
                        }
                        else
                        {
                            _logger.LogError("Failed to register subscription: {Error}", 
                                string.Join(", ", subResult.Error.Errors.Select(x => $"{x.Code}: {x.Message}")));
                        }
                    }
                }
                else
                {
                    _logger.LogError("Failed to check subscription: {Error}", 
                        string.Join(", ", checkSubResult.Error.Errors.Select(x => $"{x.Code}: {x.Message}")));
                }
            }
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
