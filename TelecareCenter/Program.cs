// ----------------------------------------------------------------------------
// <copyright file="Program.cs" company="SOGETI Spain">
//     Copyright © 2015 SOGETI Spain. All rights reserved.
//     Connect Your IoT Device to the Cloud by Osc@rNET.
// </copyright>
// ----------------------------------------------------------------------------
namespace TelecareCenter
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;
    using TelecareShared;

    /// <summary>
    /// Represents the main program.
    /// </summary>
    public class Program
    {
        #region Fields

        /// <summary>
        /// Defines the queue client.
        /// </summary>
        private static QueueClient client = null;

        #endregion Fields

        #region Methods

        /// <summary>
        /// The entry point of the main program.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            Console.Title = "SOGETI Telecare Center";
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Clear();
            Console.WriteLine("Connect Your IoT Device to the Cloud by Osc@rNET.");
            Console.WriteLine("© 2015 SOGETI Spain. Please visit our blog: 'itblogsogeti.com'.");
            Console.WriteLine();
            Console.WriteLine("LIST OF EMERGENCY ALERTS");
            Console.WriteLine();
            Console.WriteLine("Created At           Person ID                         Person (Full Name)");
            Console.WriteLine(string.Empty.PadLeft(80, '_'));

            // Initialize message pump options.
            OnMessageOptions options = new OnMessageOptions();
            options.AutoComplete = true; // Indicates if the message-pump should call complete on messages after the callback has completed processing.
            options.MaxConcurrentCalls = 1; // Indicates the maximum number of concurrent calls to the callback the pump should initiate.
            options.ExceptionReceived += Program.LogErrors; // Enables you to get notified of any errors encountered by the message pump.

            QueueClient client = QueueClient.CreateFromConnectionString(
                QueueConfig.BusConnectionString,
                QueueConfig.BusQueueName);

            client.OnMessage(
                receivedMessage =>
                {
                    try
                    {
                        Debug.WriteLine("Trying to receive message...");

                        EmergencyNotification notification = null;
                        using (var reader = new StreamReader(receivedMessage.GetBody<Stream>(), Encoding.UTF8))
                        {
                            var bodyJson = reader.ReadToEnd();
                            notification = JsonConvert.DeserializeObject<EmergencyNotification>(bodyJson);
                        }

                        Console.WriteLine(
                            "{0:yyyy-MM-dd HH:mm:ss}  {1:N}  {2}",
                            notification.CreatedAt.ToLocalTime(),
                            notification.PersonID,
                            notification.PersonFullName);

                        Debug.WriteLine("The message was received successfully.");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("Failed to receive message: {0}", exception.Message);
                    }
                },
                options);

            while (true) ;
        }

        /// <summary>
        /// Occurs when exception is received.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event data.</param>
        private static void LogErrors(object sender, ExceptionReceivedEventArgs e)
        {
            if (e.Exception != null)
            {
                Console.WriteLine("Error: " + e.Exception.Message);
                client.Close();
            }
        }

        #endregion Methods
    }
}
