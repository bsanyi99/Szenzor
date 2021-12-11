using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Server;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {

        List<ClientInfo> clientList;
        Socket serverSocket;
        public static int byteDataSize = 2200000;
        byte[] byteData = new byte[byteDataSize];


        public MainWindow()
        {
            clientList = new List<ClientInfo>();
            InitializeComponent();
        }


        //System Logging delegate
        private delegate void UpdateDelegate(string pMessage);


        //UpdateMessage metódus frissíti a tevékenyésgeket a szerveren
        private void UpdateMessage(string pMessage)
        {
            this.systemLog.Text += pMessage;
            this.systemLog.ScrollToEnd();

            systemLog.SelectionStart = systemLog.Text.Length;
            systemLog.ScrollToEnd();
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                //Server socket létrehozása, TCP
                serverSocket = new Socket(AddressFamily.InterNetwork,
                                          SocketType.Stream,
                                          ProtocolType.Tcp);

                //IpEndpoint létrehozása, 1000-es port számmal
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 1000);

                //Bind and listen
                serverSocket.Bind(ipEndPoint);
                serverSocket.Listen(10);

                //Beérkező kliens kérelmek fogadása
                serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TCPserver");
            }
        }

        private void OnAccept(IAsyncResult ar)
        {
            try
            {
                //Socket létrehozása a kapcsolódott kliensnek
                Socket clientSocket = serverSocket.EndAccept(ar);

                //Várakozás, további kliensek fogadása
                serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

                //Adat fogadásának megkezdése a klienstől, miután csatlakozott
                clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                    new AsyncCallback(OnReceive), clientSocket);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TCPserver");
            }
        }

        private void OnReceive(IAsyncResult ar)
        {

            try
            {
                //Csatlakozott kliens socketja
                Socket clientSocket = (Socket)ar.AsyncState;
                clientSocket.EndReceive(ar);


                //Beérkezett parancs vizsgálata és a megfelelő műveletek végrehajtása

                byte[] commandType = new byte[4];
                Buffer.BlockCopy(byteData, 0, commandType, 0, 4);

                Data msgReceived = new Data(byteData);
                Data msgToSend = new Data();
                byte[] message = new byte[byteDataSize];

                switch (msgReceived.cmdCommand)
                {

                    //Login command
                    case Command.Login:

                        ClientInfo client = new ClientInfo(clientSocket, msgReceived.strName);
                        clientList.Add(client);


                        msgToSend.cmdCommand = Command.Accept;
                        msgToSend.strMessage = "Sikeres kapcsolódás!";
                        message = msgToSend.toByte();

                        clientSocket.BeginSend(message, 0, message.Length, SocketFlags.None,
                                    new AsyncCallback(OnSend), clientSocket);


                        UpdateDelegate updateLogin = new UpdateDelegate(UpdateMessage);
                        this.systemLog.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, updateLogin,
                           "Smart home activated" + "\n");

                        break;

                    //Logout command
                    case Command.Logout:
                        int clientIndex = 0;

                        foreach (ClientInfo Client in clientList)
                        {
                            if (Client.clientSocket == clientSocket)
                            {

                                clientList.RemoveAt(clientIndex);
                                break;

                            }
                            ++clientIndex;
                        }

                        clientSocket.Close();

                        UpdateDelegate updateLogout = new UpdateDelegate(UpdateMessage);
                        this.systemLog.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, updateLogout,
                            "Smart home deactivated! \n");

                        break;


                    //Public Message
                    case Command.Message:
                        msgToSend.cmdCommand = msgReceived.cmdCommand;
                        msgToSend.strMessage = msgReceived.strMessage;
                        msgToSend.strName = msgReceived.strName;

                        message = msgToSend.toByte();


                        foreach (ClientInfo Client in clientList)
                        {
                            if (Client.clientSocket != clientSocket)
                                Client.clientSocket.Send(message, 0, message.Length, SocketFlags.None);
                        }


                        UpdateDelegate updatePubMsg = new UpdateDelegate(UpdateMessage);
                        this.systemLog.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, updatePubMsg,
                       msgReceived.strMessage + "\n");

                        break;



                }


                //További üzenetek fogadása a klienstől ha a beérkezett üzenetben szereplő parancs nem Logout command
                if (BitConverter.ToInt32(commandType, 0) != (int)Command.Logout)
                {
                    clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSserverTCP1");
            }

        }

        public void OnSend(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndSend(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TCPserver");
            }
        }


    }

    class ClientInfo
    {
        public Socket clientSocket;
        public string clientName;

        public ClientInfo(Socket clientSocket, string clientName)
        {
            this.clientSocket = clientSocket;
            this.clientName = clientName;
        }
    }

}


