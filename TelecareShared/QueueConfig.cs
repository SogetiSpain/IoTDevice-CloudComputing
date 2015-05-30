// ----------------------------------------------------------------------------
// <copyright file="QueueConfig.cs" company="SOGETI Spain">
//     Copyright © 2015 SOGETI Spain. All rights reserved.
//     Connect Your IoT Device to the Cloud by Osc@rNET.
// </copyright>
// ----------------------------------------------------------------------------
namespace TelecareShared
{
    /// <summary>
    /// Represents the queue configuration.
    /// </summary>
    public static class QueueConfig
    {
        #region Fields

        /// <summary>
        /// Defines the connection string (Azure Service Bus).
        /// </summary>
        public const string BusConnectionString =
            "Endpoint=sb://iotdevice-cloudcomputing-ns.servicebus.windows.net/;SharedAccessKeyName=SubmitAndProcess;SharedAccessKey=wng3EFI83uz5SrPF8Ie+IEG/d+Db8XewFO8Dk+ihigc=";

        /// <summary>
        /// Defines the queue name (Azure Service Bus).
        /// </summary>
        public const string BusQueueName = "IoTDevice-CloudComputing";

        /// <summary>
        /// Defines the queue namespace (Azure Service Bus).
        /// </summary>
        public const string BusQueueNamespace = "IoTdevice-CloudComputing-ns";

        /// <summary>
        /// Defines the shared access key (Azure Service Bus).
        /// </summary>
        public const string BusSharedAccessKey = "wng3EFI83uz5SrPF8Ie+IEG/d+Db8XewFO8Dk+ihigc=";

        /// <summary>
        /// Defines the shared access key name (Azure Service Bus).
        /// </summary>
        public const string BusSharedAccessKeyName = "SubmitAndProcess";

        #endregion Fields
    }
}
