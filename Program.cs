using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.ServiceBus.Messaging;

namespace IotEventHubReceiver
{
    class Program
    {
        class evData
        {
            public string id { get; set; }
            public string data { get; set; }
        }

        static string connectionString = "IOT_HUB_CONNECTION_STRING";
        static string iotHubD2cEndpoint = "messages/events";
        static EventHubClient eventHubClient;

        static void Main(string[] args)
        {
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);
            SynchMessages();
        }

        private static async void SynchMessages()
        {

            while (true)
            {
                var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

                foreach (string partition in d2cPartitions)
                {
                    ReceiveMessagesFromDeviceAsync(partition);
                }
            }
        }

        private async static Task ReceiveMessagesFromDeviceAsync(string partition)
        {

            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);
            while (true)
            {
                EventData eventData = await eventHubReceiver.ReceiveAsync();
                if (eventData == null) continue;

                string data = Encoding.UTF8.GetString(eventData.GetBytes());

                evData d = JsonConvert.DeserializeObject<evData>(data);

                StreamWriter writer = new StreamWriter("c:\\gps\\download\\"
                    + d.id + "." + DateTime.Now.Year + "." + DateTime.Now.Month
                    + "." + DateTime.Now.Day + "." + DateTime.Now.Hour + "."
                    + DateTime.Now.Minute + "." + DateTime.Now.Second + ".csv");

                writer.Write(d.data);
                writer.Close();
                Console.WriteLine(string.Format("Message received from {0}.\n{1}", d.id, d.data));
            }
        }
    }
}
