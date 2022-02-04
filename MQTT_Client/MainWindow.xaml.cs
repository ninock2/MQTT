using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;


namespace MQTT_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MqttClient client;
        string clientId = "";


        public MainWindow()
        {
            InitializeComponent();

            string BrokerAddress = "127.0.0.1";

            client = new MqttClient(BrokerAddress);

            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            clientId = Guid.NewGuid().ToString();

            client.Connect(clientId);


            labelClientID.Content = clientId;
            labelBrokerIP.Content = BrokerAddress.ToString();

            if (client.IsConnected)
            {
                labelIsConnected.Content = "Client is Connected to Broker";
                WriteLog("Connection","Client is Connected to Broker", listBoxLogger);
            }
            else
            {
                labelIsConnected.Content = "Client is NOT Connected to Broker";
                WriteLog("Connection", "Client is NOT Connected to Broker", listBoxLogger);
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            client.Disconnect();

            base.OnClosed(e);
            MessageBox.Show(e.ToString());
            App.Current.Shutdown();
        }


        private void btnSubscribe_Click(object sender, RoutedEventArgs e)
        {
            if (txtTopicSubscribe.Text != "")
            {
                string Topic = txtTopicSubscribe.Text;


                client.Subscribe(new string[] { Topic }, new byte[] { 2 }); 

                listBoxSubscribedTopics.Items.Add(Topic);
                
                WriteLog("Subscription",Topic, listBoxLogger);
            }
            else
            {
                System.Windows.MessageBox.Show("You have to enter a topic to subscribe!");
                WriteLog("Subscription", "Invalid Topic!", listBoxLogger);
            }
        }


        private void btnPublish_Click(object sender, RoutedEventArgs e)
        {
            if (txtTopicPublish.Text != "")
            {
                string Topic = txtTopicPublish.Text;

                client.Publish(Topic, Encoding.UTF8.GetBytes(txtPublish.Text), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
                WriteLog("Publishing",Topic+": "+txtPublish.Text, listBoxLogger);

            }
            else
            {
                System.Windows.MessageBox.Show("You have to enter a topic to publish!");
                WriteLog("Publishing", "You have to enter a topic to publish!", listBoxLogger);
            }
        }


        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string ReceivedMessage = Encoding.UTF8.GetString(e.Message);
            string topicName = e.Topic.ToString();

            string topicMessage = topicName + ": " + ReceivedMessage;

            Dispatcher.Invoke(delegate
            {              
                listBoxRecived.Items.Add(topicMessage);
                WriteLog("Recived message",topicMessage, listBoxLogger);
            });
        }

        public static void WriteLog(string typeOfLog, string message, ListBox list)
        {
            string log = typeOfLog + "; " + message + "; " + DateTime.Now.ToString();

            list.Items.Add(log);
        }
    }
}
