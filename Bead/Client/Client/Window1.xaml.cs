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
using System.Windows.Threading;

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
        bool isNappal = false;
        bool isEste = false;

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

            Data msgReceived = new Data(byteData);


            if (msgReceived.cmdCommand == Command.Message)
            {

                string msg = msgReceived.strName + " Üzeni " + msgReceived.strMessage + "\n";

            }
            else if (msgReceived.cmdCommand == Command.List)
            {
                string msg = msgReceived.strMessage;

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

        private void button1_Click(object sender, RoutedEventArgs e)
        {

            Thread t = new Thread(new ThreadStart(Work));
            t.Start();


            ////// Ez az lámpa inditó
            Thread t2 = new Thread(new ThreadStart(Clock));
            t2.Start();


        }



        private void Work()
        {
            int kinti = 1;
            int label_ertek = 0;
            int allando = 20;
            bool isFutes = false;

            this.Dispatcher.Invoke((Action)(() =>
            {//this refer to form in WPF application 
                string s = this.kint_ho.Text;
                string s1 = this.label1.Content.ToString();
                kinti = int.Parse(s);
                label_ertek = int.Parse(s1);


            }));
            if (kinti != allando)
            {
                if (kinti < allando)
                {
                    /**Fűtés**/

                    int i = label_ertek;
                    while (i >= label_ertek - 4)
                    {
                        Updater uiUpdater = new Updater(UpdateUI);
                        Dispatcher.BeginInvoke(DispatcherPriority.Send, uiUpdater, i);
                        Thread.Sleep(300);
                        i--;
                    }

                    this.Dispatcher.Invoke(delegate ()
                    {
                        label1.Background = Brushes.Red;
                    });

                    for (int j = i; j <= allando; j++)
                    {

                        Updater uiUpdater = new Updater(UpdateUI);
                        Dispatcher.BeginInvoke(DispatcherPriority.Send, uiUpdater, j);
                        Thread.Sleep(300);
                    }
                    this.Dispatcher.Invoke(delegate ()
                    {
                        label1.Background = Brushes.Transparent;
                    });
                }
                else
                {
                    /**Hűtés**/
                    int i = label_ertek;    /// 20 fok
                    while (i <= label_ertek + 4)            ///20 <= 24
                    {
                        Updater uiUpdater = new Updater(UpdateUI);
                        Dispatcher.BeginInvoke(DispatcherPriority.Send, uiUpdater, i);
                        Thread.Sleep(300);
                        i++;
                    }

                    this.Dispatcher.Invoke(delegate ()
                    {
                        label1.Background = Brushes.Blue;
                    });
                    for (int j = i; j >= allando; j--)          /// 24 --> 20 -->  
                    {

                        Updater uiUpdater = new Updater(UpdateUI);
                        Dispatcher.BeginInvoke(DispatcherPriority.Send, uiUpdater, j);
                        Thread.Sleep(300);

                    }
                    this.Dispatcher.Invoke(delegate ()
                    {
                        label1.Background = Brushes.Transparent;
                    });


                }
            }
        }

        private delegate void Updater(int UI);

        private void UpdateUI(int i)
        {
            label1.Content = i;
        }

        private void kint_ho_TextChanged(object sender, TextChangedEventArgs e)
        {

        }



        /***********************ÓRA***************************/
        private void Clock()
        {
            int alap_ora1 = 0;

            this.Dispatcher.Invoke((Action)(() =>
            {//this refer to form in WPF application 

                string alapora = this.Ora.Content.ToString();
                alap_ora1 = int.Parse(alapora);


            }));
            int i = alap_ora1;
            while (true)
            {

                Updater3 uiUpdater3 = new Updater3(ClockUpdait);
                Dispatcher.BeginInvoke(DispatcherPriority.Send, uiUpdater3, i);
                Thread.Sleep(2000);
                i++;
                if (i == 25)
                {
                    i = 0;
                }

                if (i >= 7 && i <= 17)
                {
                    isNappal = true;
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        this.LampLights.Text = "Lights off";
                    }));
                }
                else
                {
                    isNappal = false;
                }

                if (i >= 18 && i <= 23)
                {
                    isEste = true;
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        this.LampLights.Text = "Lights on";
                    }));

                }
                else
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        this.LampLights.Text = "Lights off";
                    }));
                    isEste = false;

                }
            }
        }



        private delegate void Updater3(int UI);

        private void ClockUpdait(int i)
        {
            Ora.Content = i;
        }



        void TextBlock_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {


            if (isNappal == false && isEste == false)
            {
                this.LampLights.Text = "Lights on";

                Thread t = new Thread(new ThreadStart(LampaValtas));
                t.Start();
            }

        }

        void LampaValtas()
        {
            Thread.Sleep(1000);
            Updater2 uiUpdater = new Updater2(UpdateUI2);
            Dispatcher.BeginInvoke(DispatcherPriority.Send, uiUpdater, "Lights off");



        }

        delegate void Updater2(string UI);

        void UpdateUI2(string i)
        {
            this.LampLights.Text = i;

        }




        /***********************************************ÓRA*********************************************/





        void TextBlock_IsMouseDirectlyOverChanged2(object sender, DependencyPropertyChangedEventArgs e)
        {


 

        }


    }
}
