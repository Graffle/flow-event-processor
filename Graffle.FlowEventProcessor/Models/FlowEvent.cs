using System;
using System.Text.Json.Serialization;

namespace Graffle.FlowEventProcessor.Models
{
    public class ProcessedFlowEvent
    {
        /// <summary>
        /// The unique contract event name. Ex: A.c1e4f4f4c4257510.Market.MomentListed
        /// </summary>
        /// <value></value>
        [JsonPropertyName("flowEventId")]
        public string FlowEventId { get; init; }

         /// <summary>
        /// The Id for the transaction that owns the event
        /// </summary>
        /// <value></value>
        [JsonPropertyName("flowTransactionId")]
        public string FlowTransactionId { get; init; }

        /// <summary>
        /// Flow Unique Sha3 token representing the block
        /// </summary>
        /// <value></value>
        [JsonPropertyName("flowBlockId")]
        public string FlowBlockId { get; init; }

        /// <summary>
        /// Block height in the flow blockchain
        /// </summary>
        /// <value></value>
        [JsonPropertyName("blockHeight")]
        public ulong BlockHeight { get; init; }

        /// <summary>
        /// Flow Timestamp on when the block executed
        /// </summary>
        /// <value></value>
        [JsonPropertyName("eventDate")]
        public DateTimeOffset EventDate { get; init; }

        /// <summary>
        /// Data gathered from the flow event in the block
        /// </summary>
        /// <value></value>
        [JsonPropertyName("blockEventData")]
        public dynamic BlockEventData { get; init; }
    }
}