# flow-event-processor
A processor to monitor events in the flow blockchain

Configuration:
- FlowNode: MainNet or TestNet
- MaximumBlockScanRange: Block size to scan when catching up after falling behind, should normally be set to 200.
- WebhookUrl: Your url which will receive POST requests containing your processed events.
- EventId: Flow event Id of the event type you want to process. ex: A.c1e4f4f4c4257510.Market.MomentPurchased
- Verbose: Log found events to the console when posting to WebhookUrl.


Running from VS Code:

Create an appsettings.local.json file in the Graffle.FlowEventProcessor folder:
```
{
    "FlowNode": "MainNet",
    "MaximumBlockScanRange": 200,
    "WebhookUrl": "https://<your url>",
    "EventId": "A.c1e4f4f4c4257510.Market.MomentPurchased",
    "Verbose": "true"
}
```

In the "Run and Debug" menu in VS Code, click the green arrow next to "Debug Graffle.FlowEventProcessor".

Running with docker-compose:

Create a .env.local file in the root of your repository, next to docker-compose.yml:
```
WEBHOOKURL=https://<your url>
EVENTID=A.c1e4f4f4c4257510.Market.MomentPurchased
VERBOSE=true
```

Open a terminal window in the root of your repository and run either:
```
 docker-compose --env-file .\.env.local up test_net_graffle_flow_event_processor
```
or
```
 docker-compose --env-file .\.env.local up main_net_graffle_flow_event_processor
```
for test net or main net event processing.