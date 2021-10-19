using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {

        public Socket clientSocket;
        public string userName;
        public string[] users;
        byte[] byteData = new byte[2048];

        private delegate void UpdateMessageDelegate(string pMessage);
        private delegate void UpdateOnlineUsersDelegate(string pMessage);
        private delegate void SendLogoutMessageDelegate();

        public Window1()
        {
            InitializeComponent();
        }
        public Window1(Socket clientSocket, String userName)
        {
            InitializeComponent();
            this.clientSocket = clientSocket;
            this.userName = userName;


            clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnRecive), clientSocket);
        }

        private void OnRecive(IAsyncResult ar)
        {

            byte[] commandType = new byte[4];

            Buffer.BlockCopy(byteData, 0, commandType, 0, 4);
            //BitConverter.ToInt32(commandType, 0);

            Data dataReceived = new Data(byteData);


            if (dataReceived.cmdCommand == Command.Message)
            {

                string msg = dataReceived.strName + " Üzeni " + dataReceived.strMessage + "\n";

            }
            else if (dataReceived.cmdCommand == Command.List)
            {
                string msg = dataReceived.strMessage;

            }

            clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnRecive), clientSocket);


        }



        private void Kitchen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Bath_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Living_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BadRoom_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Heat_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(new ThreadStart(Heating));
            t.Start();
            Data dataToSend = new Data();
            dataToSend.cmdCommand = Command.Heat;

            byte[] data = dataToSend.toByte();
            clientSocket.Send(data);

        }

        private void Heating()
        {
            int temp = 25;
            for (int i = 0; i < 46; i++)
            {
                Console.WriteLine($"{i}.perc utan a viz homerseklete: {temp}°C");
                Thread.Sleep(1000);
                temp += 1;
            }
            
        }

        private void Cool_Click(object sender, RoutedEventArgs e)
        {
            Data dataToSend = new Data();
            dataToSend.cmdCommand = Command.Cool;

            byte[] data = dataToSend.toByte(); 
            clientSocket.Send(data);

        }


        private void Logout(object sender, RoutedEventArgs e)
        {
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.Logout;
            msgToSend.strName = userName;

            byte[] message = new byte[2200000];
            message = msgToSend.toByte();
            clientSocket.Send(message);
            Close();
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Fullscreen(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }
    }
}
