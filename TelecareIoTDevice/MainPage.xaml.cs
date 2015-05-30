// ----------------------------------------------------------------------------
// <copyright file="MainPage.xaml.cs" company="SOGETI Spain">
//     Copyright © 2015 SOGETI Spain. All rights reserved.
//     Connect Your IoT Device to the Cloud by Osc@rNET.
// </copyright>
// ----------------------------------------------------------------------------
namespace TelecareIoTDevice
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using TelecareShared;
    using Windows.Devices.Gpio;
    using Windows.Security.Cryptography;
    using Windows.Security.Cryptography.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// Represents the main page.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Fields

        /// <summary>
        /// Defines the GPIO number for the buzzer.
        /// </summary>
        private const int BuzzerGpioNumber = 16;

        /// <summary>
        /// Defines the GPIO number for the green light (RGB LED).
        /// </summary>
        private const int GreenLightGpioNumber = 23;

        /// <summary>
        /// Defines the GPIO number for the red light (RGB LED).
        /// </summary>
        private const int RedLightGpioNumber = 18;

        /// <summary>
        /// Defines the GPIO number for the switch.
        /// </summary>
        private const int SwitchGpioNumber = 24;

        /// <summary>
        /// Represents the GPIO pin for the buzzer.
        /// </summary>
        private GpioPin buzzerPin = null;

        /// <summary>
        /// Represents the GPIO pin for the green light (RGB LED).
        /// </summary>
        private GpioPin greenLightPin = null;

        /// <summary>
        /// Represents the GPIO pin for the red light (RGB LED).
        /// </summary>
        private GpioPin redLightPin = null;

        /// <summary>
        /// Represents the GPIO pin for the switch.
        /// </summary>
        private GpioPin switchPin = null;

        /// <summary>
        /// Represents the GPIO pin old value of the switch.
        /// </summary>
        private GpioPinValue? switchPinOldValue = null;

        /// <summary>
        /// Represents the timer.
        /// </summary>
        private DispatcherTimer timer = null;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += this.OnMainPageLoaded;
            this.Unloaded += this.OnMainPageUnloaded;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Creates a shared access signature token (for Azure Bus Service).
        /// </summary>
        /// <returns>
        /// The created SAS token.
        /// </returns>
        private string CreateSASToken()
        {
            var expiry = (int)DateTime.UtcNow.AddMinutes(20).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            var uri = string.Format("{0}.servicebus.windows.net", QueueConfig.BusQueueNamespace);
            var stringToSign = WebUtility.UrlEncode(uri) + "\n" + expiry.ToString();
            var signature = this.HmacSha256(QueueConfig.BusSharedAccessKey, stringToSign);

            var token = String.Format(
                "sr={0}&sig={1}&se={2}&skn={3}",
                WebUtility.UrlEncode(uri),
                WebUtility.UrlEncode(signature),
                expiry,
                QueueConfig.BusSharedAccessKeyName);

            return token;
        }

        /// <summary>
        /// Finalizes the GPIO of the IoT device.
        /// </summary>
        private void FinalizeGPIO()
        {
            Debug.WriteLine("Trying to finalize the GPIO...");

            if (this.buzzerPin != null)
            {
                this.buzzerPin.Dispose();
                this.buzzerPin = null;
            }

            if (this.greenLightPin != null)
            {
                this.greenLightPin.Dispose();
                this.greenLightPin = null;
            }

            if (this.redLightPin != null)
            {
                this.redLightPin.Dispose();
                this.redLightPin = null;
            }

            Debug.WriteLine("The GPIO was finalized successfully.");
        }

        /// <summary>
        /// Finalizes the timer.
        /// </summary>
        private void FinalizeTimer()
        {
            Debug.WriteLine("Trying to finalize the timer...");

            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer.Tick -= this.OnTimerTick;
                this.timer = null;
            }

            Debug.WriteLine("The timer was finalized successfully.");
        }

        /// <summary>
        /// Calculates the HMAC-SHA256.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The calculated HMAC-SHA256.
        /// </returns>
        private string HmacSha256(string key, string value)
        {
            var keyStrm = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
            var valueStrm = CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8);

            var objMacProv = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
            var hash = objMacProv.CreateHash(keyStrm);
            hash.Append(valueStrm);

            return CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset());
        }

        /// <summary>
        /// Initializes the GPIO of the IoT device.
        /// </summary>
        /// <returns>
        ///   <b>true</b> if the GPIO was initialized successfully; otherwise, <b>false</b>.
        /// </returns>
        private bool InitializeGPIO()
        {
            GpioController gpioController = null;
            Debug.WriteLine("Trying to initialize the GPIO...");

            try
            {
                gpioController = GpioController.GetDefault();
            }
            catch (Exception exception)
            {
                Debug.WriteLine("The GPIO failed to initialize. {0}", exception.Message);
            }

            if (gpioController == null)
            {
                this.IoTDeviceStatusTextBlock.Text = "Warning: the telecare device failed to initialize.";
                return false;
            }

            this.buzzerPin = gpioController.OpenPin(MainPage.BuzzerGpioNumber);
            this.buzzerPin.Write(GpioPinValue.Low);
            this.buzzerPin.SetDriveMode(GpioPinDriveMode.Output);


            this.greenLightPin = gpioController.OpenPin(MainPage.GreenLightGpioNumber);
            this.greenLightPin.Write(GpioPinValue.Low);
            this.greenLightPin.SetDriveMode(GpioPinDriveMode.Output);

            this.redLightPin = gpioController.OpenPin(MainPage.RedLightGpioNumber);
            this.redLightPin.Write(GpioPinValue.Low);
            this.redLightPin.SetDriveMode(GpioPinDriveMode.Output);


            this.switchPin = gpioController.OpenPin(MainPage.SwitchGpioNumber);
            this.switchPin.SetDriveMode(GpioPinDriveMode.Output);

            this.IoTDeviceStatusTextBlock.Text = "The telecare device initialized correctly.";
            Debug.WriteLine("The GPIO was initialized successfully.");

            return true;
        }

        /// <summary>
        /// Initializes the timer.
        /// </summary>
        private void InitializeTimer()
        {
            Debug.WriteLine("Trying to initialize the timer...");

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(250);
            this.timer.Tick += this.OnTimerTick;
            this.timer.Start();

            Debug.WriteLine("The timer was initialized successfully.");
        }

        /// <summary>
        /// Occurs when the main page is loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">An <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private void OnMainPageLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("The main page was loaded successfully.");

            if (this.InitializeGPIO())
            {
                this.InitializeTimer();
            }
        }

        /// <summary>
        /// Occurs when the main page is unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">An <see cref="RoutedEventArgs"/> that contains the event data.</param>
        private void OnMainPageUnloaded(object sender, RoutedEventArgs e)
        {
            this.FinalizeTimer();
            this.FinalizeGPIO();

            Debug.WriteLine("The main page was unloaded successfully.");
        }

        /// <summary>
        /// Occurs when the specified timer interval has elapsed and the timer is enabled.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event data.</param>
        private void OnTimerTick(object sender, object e)
        {
            if (this.switchPin == null)
            {
                return;
            }

            var currentSwitchPinValue = this.switchPin.Read();

            if (this.switchPinOldValue != currentSwitchPinValue)
            {
                this.switchPinOldValue = currentSwitchPinValue;

                if (currentSwitchPinValue == GpioPinValue.High)
                {
                    this.TurnOnGreenLight();
                }
                else
                {
                    Task.Factory.StartNew(
                        () => this.SendEmergencyNotification());
                    this.TurnOnRedLight();
                    this.TurnOnBuzzer();
                }
            }
        }

        /// <summary>
        /// Sends an emergency notification.
        /// </summary>
        private void SendEmergencyNotification()
        {
            Debug.WriteLine("Trying to send a notification to service bus queue.");

            var notification = new EmergencyNotification()
            {
                CreatedAt = DateTime.UtcNow,
                PersonFullName = "OSCAR FERNANDEZ GONZALEZ",
                PersonID = Guid.NewGuid()
            };

            try
            {
                var baseUri = string.Format(
                    "https://{0}.servicebus.windows.net",
                    QueueConfig.BusQueueNamespace);

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseUri);
                    client.DefaultRequestHeaders.Accept.Clear();

                    var token = this.CreateSASToken();
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("SharedAccessSignature", token);

                    var json = JsonConvert.SerializeObject(notification);
                    var content = new StringContent(json, Encoding.UTF8);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var path = string.Format("/{0}/messages", QueueConfig.BusQueueName);

                    var response = client.PostAsync(path, content).Result;

                    Debug.WriteLine(response.IsSuccessStatusCode
                        ? "A notification was sent successfully to service bus queue."
                        : "Failed to send a notification to service bus queue.");

                    Debug.WriteLine(
                        "{0:yyyy-MM-dd HH:mm:ss}  {1:N}  {2}",
                        notification.CreatedAt.ToLocalTime(),
                        notification.PersonID,
                        notification.PersonFullName);
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(
                    "Failed to send a notification to service bus queue. {0}",
                    exception.Message);
            }
        }

        /// <summary>
        /// Turns on the buzzer.
        /// </summary>
        private void TurnOnBuzzer()
        {
            while (this.switchPin.Read() != GpioPinValue.High)
            {
                this.buzzerPin.Write(GpioPinValue.High);
                this.buzzerPin.Write(GpioPinValue.Low);
            }
        }

        /// <summary>
        /// Turns on the green light.
        /// </summary>
        private void TurnOnGreenLight()
        {
            this.greenLightPin.Write(GpioPinValue.High);
            this.redLightPin.Write(GpioPinValue.Low);

            Debug.WriteLine("The GREEN light turns on!");
        }

        /// <summary>
        /// Turns on the red light.
        /// </summary>
        private void TurnOnRedLight()
        {
            this.greenLightPin.Write(GpioPinValue.Low);
            this.redLightPin.Write(GpioPinValue.High);

            Debug.WriteLine("The RED light turns on!");
        }

        #endregion Methods
    }
}
