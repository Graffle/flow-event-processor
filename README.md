# flow-event-processor
A processor to monitor events in the flow blockchain

Configuration:
- FlowNode: MainNet, TestNet, or Emulator. Note: Emulator support is currently limited to running from VS Code.
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

Open a terminal window in the root of your repository and run one of the following:
```
 docker-compose build test_net_graffle_flow_event_processor
 docker-compose --env-file .\.env.local up test_net_graffle_flow_event_processor
```
```
 docker-compose build main_net_graffle_flow_event_processor
 docker-compose --env-file .\.env.local up main_net_graffle_flow_event_processor
```
for test net or main net event processing.

Running with docker:

From the Graffle.FlowEventProcessor folder, run:
Note: you can replace "localdev" with a tag name that makes more sense for your use case in the following commands
```
docker build -t localdev -f .\Dockerfile.development .
```
and then
```
docker run -e "EventId=<your event Id here>" -e "WebhookUrl=<your webhook url here>" -e "FlowNode=MainNet" localdev
```
or add Verbose=True to log when events are sent to your webhook url.
```
docker run -e "EventId=<your event Id here>" -e "WebhookUrl=<your webhook url here>" -e "Verbose=True" -e "FlowNode=MainNet" localdev
```
You can also run against TestNet by running:
```
docker run -e "EventId=<your event Id here>" -e "WebhookUrl=<your webhook url here>" -e "FlowNode=TestNet" -e "Verbose=True" localdev
```

<a href="https://github.com/Graffle/flow-event-processor/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=Graffle/flow-event-processor" />
</a>

Made with [contrib.rocks](https://contrib.rocks).
