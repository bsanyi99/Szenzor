using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Client.Registration;
using Newtonsoft.Json;
using System.Net;

namespace Client
{
    /// <summary>
    /// Interaction logic for login.xaml
    /// </summary>
    public partial class login : Window
    {
        List<Users> users;
        public Socket clientSocket;
        public string userName;

        public delegate string getNameDelegate();
        public delegate void newMsgWindowDelegate();

        public login()
        {
            InitializeComponent();

        }

        public string getLoginName()
        {
            return this.userBox.Text.ToString();
        }

        private void newMsgWindow()
        {
            Window1 newMsgWindow = new Window1(clientSocket, userName);
            newMsgWindow.Show();

            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void RegistrationButtonHandler(object sender, RoutedEventArgs e)
        {
            Registration registration = new Registration();
            registration.Show();
            Close();
        }



        private void CloseButtonHandler(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoginButtonHandler(object sender, RoutedEventArgs e)
        {
            users = new List<Users>();
            Boolean wrongPasswd = false;
            userName = userBox.Text;
            string password = passBox.Password;

            string json = File.ReadAllText("../../users.json");

            users = JsonConvert.DeserializeObject<List<Users>>(json);

            if (userName.Equals(""))
            {
                MessageBox.Show("Adja meg a felhasznalot!");
            }else if (password.Equals(""))
            {
                MessageBox.Show("Adja meg a jelszót!");
            }else if(!users.Contains(new Users(userName, password)))
            {
                MessageBox.Show("Nem létezik ilyen felhasznalo");
            }
            else
            {
                foreach(Users user in users)
                {
                    if(user.usrName.Equals(userName) && !user.passWd.Equals(password))
                    {
                        MessageBox.Show("Hibás jelszó");
                        wrongPasswd = true;
                        break;
                    }
                }
                if (!wrongPasswd)
                {
                    try
                    {
                        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

                        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

                        clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message, "TCPclient");
                    }
                }
                else
                {
                    wrongPasswd = false;
                }
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndConnect(ar);

                string l_ip;
                Data msgToSend = new Data();
                msgToSend.cmdCommand = Command.Login;
                getNameDelegate fhNev = new getNameDelegate(getLoginName);
                l_ip = (string)this.userBox.Dispatcher.Invoke(fhNev, null);

                msgToSend.strName = l_ip;

                byte[] b = new byte[5000];
                b = msgToSend.toByte();

                clientSocket.BeginSend(b, 0, b.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TCPclient");
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            try { 
            clientSocket.EndSend(ar);
            byte[] byteData = new byte[2048];

            clientSocket.Receive(byteData, 0, byteData.Length, SocketFlags.None);

            Data msg = new Data(byteData);
            if (msg.cmdCommand == Command.Accept)
                {
                    //Sikeres csatlakozás, és a messageWindow(fő ablak) megnyitása
                    newMsgWindowDelegate msgWindow = new newMsgWindowDelegate(newMsgWindow);
                    this.Dispatcher.Invoke(msgWindow, null);
            }
                else
            {
                MessageBox.Show("Sikertelen csatlakozás!");
            }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "TCPclient");
            }
        }

    }
}
