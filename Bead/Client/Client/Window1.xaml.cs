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

            clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnRecive), clientSocket);


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

            Thread t3 = new Thread(new ThreadStart(NumberAdd));
            t3.Start();


        }

        bool isParasito = false;
        private bool isForralas = false;
        double eredeti = 0;
        private void Parasito_Click(object sender, RoutedEventArgs e)
        {
            sendMessagetoServer("Párásító berendezés bekapcsolva!");
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



            this.Dispatcher.Invoke((Action)(() =>
            {//this refer to form in WPF application 
                string s = this.kint_ho.Text;
                string s1 = this.label1.Content.ToString();
                kinti = int.Parse(s);
                label_ertek = int.Parse(s1);


            }));


            if (kinti != allando)
            {
                if (kinti < allando && kinti + 3 < allando)
                {
                    /**Fűtés**/

                    int i = label_ertek;
                    while (i >= label_ertek - 4)
                    {
                        Updater uiUpdater = new Updater(UpdateUI);
                        Dispatcher.BeginInvoke(DispatcherPriority.Send, uiUpdater, i);
                        //sendMessagetoServer("Hőmérséklet lecsökkent " + i.ToString() + "°C-ra/-re");
                        Thread.Sleep(300);
                        i--;
                    }
                    try
                    {

                        this.Dispatcher.Invoke(delegate ()
                        {
                            label1.Background = Brushes.Red;
                        });


                    }
                    catch (System.Threading.Tasks.TaskCanceledException)
                    {
                        Console.WriteLine("");
                    }

                    sendMessagetoServer("Fűtés bekapcsolva!");
                    for (int j = i; j <= allando; j++)
                    {

                        Updater uiUpdater = new Updater(UpdateUI);
                        Dispatcher.BeginInvoke(DispatcherPriority.Send, uiUpdater, j);
                        Thread.Sleep(300);

                    }
                    sendMessagetoServer("Fűtés kikapcsolva!");

                    try
                    {
                        this.Dispatcher.Invoke(delegate ()
                        {
                            label1.Background = Brushes.Transparent;
                        });
                    }
                    catch (System.Threading.Tasks.TaskCanceledException)
                    {
                        Console.WriteLine("");
                    }




                }
                if (kinti > allando && kinti + 3 > allando)
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
                    sendMessagetoServer("Hűtés bekapcsolva!");
                    for (int j = i; j >= allando; j--)          /// 24 --> 20 -->  
                    {
                        Updater uiUpdater = new Updater(UpdateUI);
                        Dispatcher.BeginInvoke(DispatcherPriority.Send, uiUpdater, j);
                        Thread.Sleep(300);

                    }
                    sendMessagetoServer("Hűtés kikapcsolva!");
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
            var path = "2020data.csv";
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
            TimeSpan interval = new TimeSpan(0, 0, 1);
            
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
                                this.Dispatcher.Invoke((Action)(() =>
                                {
                                    interval = TimeSpan.FromMilliseconds(Convert.ToInt32(this.sebesseg.Text)*50);
                                }));
                                Thread.Sleep(interval);

                                if (startDate == sunDatas[i].SunRise_Time1)
                                {
                                    this.Dispatcher.Invoke((Action)(() =>
                                    {
                                        this.LampLights.Text = "Lights off";
                                        sendMessagetoServer("Külső lámpák lekapcsolva!");
                                    }));
                                }
                                else
                                {
                                    if (startDate == sunDatas[i].SunSet_Time1)
                                    {
                                        this.Dispatcher.Invoke((Action)(() =>
                                        {
                                            this.LampLights.Text = "Lights on";
                                            this.redonyAllapot.Content = "Lent";
                                            sendMessagetoServer("Külső lámpák felkapcsolva!");
                                            sendMessagetoServer("Redőny lehúzva!");
                                        }));
                                    }
                                }
                                if (startDate.Hour == 7 && startDate.Minute == 15)
                                {
                                    this.Dispatcher.Invoke((Action)(() =>
                                    {
                                        this.redonyAllapot.Content = "Fent";
                                        this.ajtoAllapot.Content = "Nyitva";
                                        sendMessagetoServer("Redőny felhúzva!");
                                        sendMessagetoServer("Ajtók nyitva!");
                                    }));
                                }
                                if (startDate.Hour == 23 && startDate.Minute == 30)
                                {
                                    this.Dispatcher.Invoke((Action)(() =>
                                    {
                                        this.ajtoAllapot.Content = "Zárva";
                                        sendMessagetoServer("Ajtók zárva!");
                                    }));
                                }
                                if (isParasito == true)
                                {
                                    this.Dispatcher.Invoke((Action)(() =>
                                    {
                                        double alap = Convert.ToDouble(this.Paratartalom.Content);
                                        alap += 0.5;
                                        if (alap == 45)
                                        {
                                            sendMessagetoServer("Párásító beavatkozó kikapcsolva!");
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
                                        if (eredeti + 6 < alap)
                                        {
                                            sendMessagetoServer("Vízforraló kikapcsolva!");
                                            isForralas = false;
                                        }
                                        else
                                        {
                                            
                                            alap += 2;
                                            this.Paratartalom.Content = alap;
                                        }
                                    }));

                                }

                                this.Dispatcher.Invoke((Action)(() =>
                                {
                                    double para = Convert.ToDouble(this.Paratartalom.Content);
                                    if (para > 45 && isForralas == false)
                                    {
                                        if (isWrite)
                                        {
                                            sendMessagetoServer("Szellőzők nyitva!");
                                            isWrite = false;
                                        }
                                        this.szellozoAllapot.Content = "ON";
                                        para -= 0.5;
                                        this.Paratartalom.Content = para;
                                        if (para == 45.5)
                                        {                                            
                                            sendMessagetoServer("Szellőzők bezárva!");
                                        }
                                        if (para == 45)
                                        {
                                            this.szellozoAllapot.Content = "OFF";
                                            isWrite = true;
                                        }
                                    }
                                }));


                            }
                            //startDate.AddHours(Convert.ToDouble(ora));                       
                        }
                        i++;
                        //startDate.AddDays(Convert.ToDouble(nap));
                    }
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




        private void Forralas_Click(object sender, RoutedEventArgs e)
        {
            sendMessagetoServer("Vízforraló bekapcsolva!");
            isForralas = true;
            eredeti = Convert.ToDouble(this.Paratartalom.Content);

        }

        private bool isOpen = false;
        private void garazsButton_Click(object sender, RoutedEventArgs e)
        {
            if (isOpen)
            {
                garazsAllapot.Content = "Lent";
                isOpen = false;
                sendMessagetoServer("Garázs bezárva!");
            }
            else
            {
                garazsAllapot.Content = "Fent";
                isOpen = true;
                sendMessagetoServer("Garázs kinyitva!");
            }


        }
        bool isDoor = false;
        bool isWrite = true;

        private void keyButton_Click(object sender, RoutedEventArgs e)
        {
            if (isDoor)
            {
                ajtoAllapot.Content = "Zárva";
                isDoor = false;

                sendMessagetoServer("Ajtók zárva!");
            }
            else
            {
                ajtoAllapot.Content = "Nyitva";
                isDoor = true;

                sendMessagetoServer("Ajtók nyitva!");
            }
        }
        private void NumberAdd()
        {

            List<int> lista = new List<int>();
            lista.Add(19);
            lista.Add(19);
            lista.Add(20);
            lista.Add(2);
            lista.Add(30);
            lista.Add(15);
            lista.Add(-7);

            for (int i = 0; i < lista.Count; i++)
            {
                Updater kintiHoUpdater = new Updater(KintiUpdater);
                Thread.Sleep(3300);
                Dispatcher.BeginInvoke(DispatcherPriority.Send, kintiHoUpdater, lista[i]);
                Thread t = new Thread(new ThreadStart(Work));
                t.Start();
            }
        }


        private void KintiUpdater(int i)
        {
            kint_ho.Text = i.ToString();
        }


        private void Logout(object sender, RoutedEventArgs e)
        {
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.Logout;
            msgToSend.strName = userName;

            byte[] message = new byte[2200000];
            message = msgToSend.toByte();
            clientSocket.Send(message);

            Environment.Exit(0);
            Close();
        }

        private void sendMessagetoServer(string uzenet)
        {
            try
            {

                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Message;
                msgToSend.strName = userName;
                msgToSend.strMessage = "<< " + uzenet;


                byte[] message = new byte[2048];
                message = msgToSend.toByte();
                clientSocket.Send(message);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("TCPclient");
            }
        }

    }
}
