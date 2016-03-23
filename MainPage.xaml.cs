using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Devices.Gpio;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client;
using System.Text;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IoTHubClientSendEvent
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private const int buttonPin = 6;
        private GpioPin buttonGPIO;
        static DeviceClient deviceClient;
        static string deviceId = "MakerChallengeDevice2";
        static string uploadFile = "data.csv";

        class evData
        {
            public string id { get; set; }
            public string data { get; set; }
        }

        public MainPage()
        {
            this.InitializeComponent();
            deviceClient = DeviceClient.CreateFromConnectionString("DEVICE_CONNECTION_STRING", TransportType.Http1);
            initGPIO();
        }

        private void initGPIO()
        {
            var gpio = GpioController.GetDefault();

            buttonGPIO = gpio.OpenPin(buttonPin);


            // Check if input pull-up resistors are supported
            if (buttonGPIO.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                buttonGPIO.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                buttonGPIO.SetDriveMode(GpioPinDriveMode.Input);

            // Set a debounce timeout to filter out switch bounce noise from a button press
            buttonGPIO.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            // Register for the ValueChanged event so our buttonPin_ValueChanged 
            // function is called when the button is pressed
            buttonGPIO.ValueChanged += buttonPin_ValueChanged;

        }

        private void buttonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            // send a message when the buttons is pressed
            if (e.Edge == GpioPinEdge.FallingEdge)
            {
                sendMessageToIOTHub();
            }
        }

        private static async void sendMessageToIOTHub()
        {
            string myString;
            var msg = new evData();
            if (File.Exists(uploadFile))
            {
                myString = System.IO.File.ReadAllText(uploadFile);
            }
            else
            {
                myString = "Just checking in!";
            }

            msg.id = deviceId;
            msg.data = myString;

            var messageString = JsonConvert.SerializeObject(msg);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);
        }
    }
}
