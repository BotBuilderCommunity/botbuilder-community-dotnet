﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Google.Helpers;
using JWT.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Bot.Builder.Community.Adapters.Google.Integration.AspNet.Core
{
    public class GoogleRequestHandler
    {
        public static readonly JsonSerializer googleBotMessageSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        });

        private readonly GoogleAdapter _googleAdapter;
        private readonly GoogleOptions _googleOptions;

        public GoogleRequestHandler(GoogleAdapter googleAdapter, GoogleOptions googleOptions)
        {
            _googleAdapter = googleAdapter;
            _googleOptions = googleOptions;
        }
       
        protected async Task<object> ProcessMessageRequestAsync(HttpRequest request, GoogleAdapter GoogleAdapter, BotCallbackHandler botCallbackHandler)
        {
            GoogleRequestBody actionRequest;
            Payload actionPayload;

            var memoryStream = new MemoryStream();
            request.Body.CopyTo(memoryStream);
            memoryStream.Position = 0;

            var projectId = AuthenticationHelpers.GetProjectIdFromRequest(request);
            GoogleAdapter.ActionProjectId = projectId;

            using (var bodyReader = new StreamReader(memoryStream, Encoding.UTF8))
            {
                var skillRequestContent = bodyReader.ReadToEnd();
                try
                {
                    actionRequest = JsonConvert.DeserializeObject<GoogleRequestBody>(skillRequestContent);
                    actionPayload = actionRequest.OriginalDetectIntentRequest.Payload;
                }
                catch
                {
                    actionPayload = JsonConvert.DeserializeObject<Payload>(skillRequestContent);
                }
            }

            var responseBody = await GoogleAdapter.ProcessActivity(
                    actionPayload,
                    botCallbackHandler);

            return responseBody;
        }

        public async Task HandleAsync(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var response = httpContext.Response;

            var requestServices = httpContext.RequestServices;
            var bot = requestServices.GetRequiredService<IBot>();

            if (request.Method != HttpMethods.Post)
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                return;
            }

            if (request.ContentLength == 0)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeaderValue)
                || mediaTypeHeaderValue.MediaType != "application/json")
            {
                response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                return;
            }

            try
            {
                var responseBody = await ProcessMessageRequestAsync(
                    request,
                    _googleAdapter,
                    bot.OnTurnAsync);
                
                var GoogleResponseBodyJson = JsonConvert.SerializeObject(responseBody, Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
                    });

                await response.WriteAsync(GoogleResponseBodyJson);
            }
            catch (UnauthorizedAccessException)
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
