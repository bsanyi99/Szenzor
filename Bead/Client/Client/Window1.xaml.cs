using System;
using System.Collections.Generic;
using System.IO;
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
            Data dataToSend = new Data();
            dataToSend.cmdCommand = Command.Heat;

            byte[] data = dataToSend.toByte();
            clientSocket.Send(data);

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
            Thread t2 = new Thread(new ThreadStart(DateClock));
            t2.Start();


        }

        bool isParasito = false;
        private bool isForralas = false;
        DateTime act_dateTime;

        private void Parasito_Click(object sender, RoutedEventArgs e)
        {
            double para = Convert.ToDouble(this.Paratartalom.Content.ToString());
            if (para < 45)
            {
                isParasito = true;
            }
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
        class SunData
        {
            private DateTime SunRise_Time;
            private DateTime SunSet_Time;

            public SunData(DateTime sunRise_Time, DateTime sunSet_Time)
            {
                SunRise_Time1 = sunRise_Time;
                SunSet_Time1 = sunSet_Time;
            }

            public DateTime SunRise_Time1 { get => SunRise_Time; set => SunRise_Time = value; }
            public DateTime SunSet_Time1 { get => SunSet_Time; set => SunSet_Time = value; }
        }




        private void DateClock()
        {
            var path = @"C:\Users\Felhasználó\Documents\GitHub\Szenzor\Bead\Client\Client\2020data.csv";
            var lines = File.ReadAllLines(path);
            List<SunData> sunDatas = new List<SunData>();
            foreach (var line in lines)
            {
                string[] tokenek = line.Split(';');
                int ev = int.Parse(tokenek[0]);
                int honap = int.Parse(tokenek[1]);
                int nap = int.Parse(tokenek[2]);
                string[] napkelte_ido = tokenek[3].Split(':');
                string[] napnyugta_ido = tokenek[4].Split(':');
                DateTime napkelte = new DateTime(ev, honap, nap, int.Parse(napkelte_ido[0]), int.Parse(napkelte_ido[1]), 0);
                DateTime napnyugta = new DateTime(ev, honap, nap, int.Parse(napnyugta_ido[0]), int.Parse(napnyugta_ido[1]), 0);
                sunDatas.Add(new SunData(napkelte, napnyugta));
            }

            int[] napok = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            DateTime startDate = new DateTime(2020, 1, 1, 0, 0, 0);
            this.Dispatcher.Invoke((Action)(() =>
            {

            }));
            int i = 0;
            while (i < sunDatas.Count())
            {
                for (int ho = 1; ho < 13; ho++)
                {
                    for (int nap = 1; nap <= napok[ho - 1]; nap++)
                    {
                        for (int ora = 0; ora < 25; ora++)
                        {
                            for (int perc = 0; perc < 60; perc++)
                            {
                                startDate = startDate.AddMinutes(1);
                                Updater5 uiUpdater5 = new Updater5(DateUpdate);
                                Dispatcher.BeginInvoke(DispatcherPriority.Send, uiUpdater5, startDate.ToString("yyyy.MM.dd"));
                                Updater3 uiUpdater3 = new Updater3(ClockUpdate);
                                Dispatcher.BeginInvoke(DispatcherPriority.Send, uiUpdater3, startDate.Hour);
                                Updater4 uiUpdater4 = new Updater4(MinUpdate);
                                Dispatcher.BeginInvoke(DispatcherPriority.Send, uiUpdater4, startDate.Minute);

                                Thread.Sleep(500);
                                if (startDate == sunDatas[i].SunRise_Time1)
                                {
                                    this.Dispatcher.Invoke((Action)(() =>
                                    {
                                        this.LampLights.Text = "Lights off";
                                    }));
                                }
                                else
                                {
                                    if (startDate == sunDatas[i].SunSet_Time1)
                                    {
                                        this.Dispatcher.Invoke((Action)(() =>
                                        {
                                            this.LampLights.Text = "Lights on";
                                        }));
                                    }
                                }
                                
                                if (isParasito == true)
                                {
                                    this.Dispatcher.Invoke((Action)(() =>
                                    {
                                        double alap = Convert.ToDouble(this.Paratartalom.Content);
                                        alap += 0.5;
                                        if (alap >= 45)
                                        {
                                            isParasito = false;
                                        }
                                        
                                        this.Paratartalom.Content = alap;
                                    }));
                                }
                                if (isForralas == true)
                                {
                                    this.Dispatcher.Invoke((Action)(() =>
                                    {
                                        double alap = Convert.ToDouble(this.Paratartalom.Content);
                                        if (act_dateTime == startDate)
                                        {
                                            isForralas = false;
                                        }
                                        else
                                        {
                                            alap += 2;
                                            this.Paratartalom.Content = alap;
                                        }
                                    }));

                                }

                            }
                            //startDate.AddHours(Convert.ToDouble(ora));                       
                        }
                        i++;
                        //startDate.AddDays(Convert.ToDouble(nap));
                    }
                }
            }
        }

        private void Clock()
        {
            int alap_ora1 = 0;

            this.Dispatcher.Invoke((Action)(() =>
            {//this refer to form in WPF application 

                string alapora = this.ora.Content.ToString();
                alap_ora1 = int.Parse(alapora);


            }));
            int i = alap_ora1;
            while (true)
            {

                Updater3 uiUpdater3 = new Updater3(ClockUpdate);
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

        private void ClockUpdate(int i)
        {
            ora.Content = i;
        }
        private delegate void Updater4(int UI);

        private void MinUpdate(int i)
        {
            if (i < 10)
            {
                perc.Content = "0" + i;
            }
            else
            {
                perc.Content = i;
            }
        }

        private delegate void Updater5(string UI);

        private void DateUpdate(string s)
        {
            datum.Text = s;
        }

        private delegate void Updater6(string UI);

        private void HumidityUpdate(string s)
        {
            Paratartalom.Content = s + "%";
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
        private void Forralas_Click(object sender, RoutedEventArgs e)
        {
            isForralas = true;
            act_dateTime = act_dateTime.AddMinutes(3);

        }
    }
}
