using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Graffle.FlowSdk;
using Graffle.FlowSdk.Services;
using Graffle.FlowSdk.Services.Models;
using Microsoft.Extensions.Configuration;
using Graffle.FlowEventProcessor.Models;
using Polly;
using System.Net.Http;

namespace Graffle.FlowEventProcessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Started At: {DateTimeOffset.UtcNow}");

            //Get the settings
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "local";
            Console.WriteLine($"Current environment: {environmentName}");
            
            var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

            Console.WriteLine($"Loaded Settings Files.");
            var nodeName = config.GetValue<string>("FlowNode");
            var maximumBlockScanRange = config.GetValue<ulong>("MaximumBlockScanRange");
            var webhookUrl = config.GetValue<string>("WebhookUrl");
            var eventId = config.GetValue<string>("EventId");
            var verbose = config.GetValue<bool>("Verbose");

            Console.WriteLine($"Target Node:      {nodeName}");
            Console.WriteLine($"Block Scan Range: {maximumBlockScanRange}");
            Console.WriteLine($"Webhook Url:      {webhookUrl}");
            Console.WriteLine($"Event Id:         {eventId}");
            Console.WriteLine($"Verbose:          {verbose}");

            Console.WriteLine($"Setup Flow Client");
            var flowClientFactory = new FlowClientFactory(nodeName);
            var flowClient = flowClientFactory.CreateFlowClient();
            Console.WriteLine($"Setup Flow Client Complete");

            Console.WriteLine($"Begin Indexing...");
            ulong lastBlockHeight = 0;
            do
            {
                try
                {
                    var blockRetryPolicy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(new[]
                        {
                            TimeSpan.FromMilliseconds(250),
                            TimeSpan.FromMilliseconds(500),
                            TimeSpan.FromMilliseconds(2000)
                        }, (exception, timeSpan, retryCount, context) =>
                        {
                            Console.WriteLine($"Encountered exception getting latest block." + 
                            $" Retry count: {retryCount}, {timeSpan}: {exception.Message} {exception.InnerException?.Message}");
                        });

                    var latestBlock = await blockRetryPolicy.ExecuteAsync<FlowBlock>(
                            () => flowClient.GetLatestBlockAsync(true)
                        );

                    var currentBlockHeight = latestBlock.Height;
                    if (lastBlockHeight == 0)
                    {
                        lastBlockHeight = currentBlockHeight;
                    }
                    else if (lastBlockHeight == currentBlockHeight)
                    {
                        //Nothing has been added to the block height. Lets pause...
                        Console.WriteLine($"{DateTimeOffset.UtcNow} - No New BLocks....Sleep");
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        var rangeDiff = currentBlockHeight - lastBlockHeight + ulong.MaxValue;
                        var outOfRange = rangeDiff > maximumBlockScanRange || currentBlockHeight == lastBlockHeight;
                        if (outOfRange)
                        {
                            currentBlockHeight = lastBlockHeight + maximumBlockScanRange;
                            Console.WriteLine("Crunching....");
                        }

                        //We have new blocks!
                        Console.WriteLine($" {DateTimeOffset.UtcNow} - Last Block: {lastBlockHeight++}, Current Block: {currentBlockHeight}");

                        var eventRetryPolicy = Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(new[]
                            {
                                TimeSpan.FromMilliseconds(250),
                                TimeSpan.FromMilliseconds(500),
                                TimeSpan.FromMilliseconds(2000)
                            }, (exception, timeSpan, retryCount, context) =>
                            {
                                Console.WriteLine($"Encountered exception getting events for block." + 
                                $" Retry count: {retryCount}, {timeSpan}: {exception.Message} {exception.InnerException?.Message}");
                            });

                        var eventsResponse = (await eventRetryPolicy.ExecuteAsync<List<Graffle.FlowSdk.Services.Models.FlowEvent>>(
                                () => flowClient.GetEventsForHeightRangeAsync(eventId, lastBlockHeight, currentBlockHeight)
                            )).GroupBy(x => x.BlockHeight).ToDictionary(x => x.Key, x => x.ToList());

                        var totalEventsFound = eventsResponse.Select(x => x.Value.Count).Sum();
                        var blocksScanned = eventsResponse.Count;

                        Console.WriteLine($" {DateTimeOffset.UtcNow} - Found {totalEventsFound} events across {blocksScanned} blocks.");

                        foreach(var blockHeightGrouping in eventsResponse) 
                        {
                            foreach (var @event in blockHeightGrouping.Value)
                            {
                                //Create the event
                                var flowEvent = new ProcessedFlowEvent()
                                {
                                    BlockEventData = @event.EventComposite.Data,
                                    BlockHeight = blockHeightGrouping.Key,
                                    EventDate = @event.BlockTimestamp,
                                    FlowBlockId = @event.BlockIdHash,
                                    FlowEventId = eventId,
                                    FlowTransactionId = @event.TransactionId.ToHash()
                                };

                                //Send webhook
                                var jsonMesage = System.Text.Json.JsonSerializer.Serialize(flowEvent);
                                using (var content = new StringContent(jsonMesage)) {
                                    using (var client = new HttpClient()) {
                                        if(verbose) {
                                            Console.WriteLine($"Posting event to {webhookUrl}: {Environment.NewLine}     {jsonMesage}");
                                        }
                                        await client.PostAsync(webhookUrl, content);
                                    }
                                }
                            }
                        }
                        lastBlockHeight = currentBlockHeight;
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                }
            } while (true);
        }
    }
}