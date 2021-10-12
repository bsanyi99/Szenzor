using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using Newtonsoft.Json;
namespace Client
{
    /// <summary>
    /// Interaction logic for Registration.xaml
    /// </summary>
    
        public partial class Registration : Window
        {
            public Registration()
            {
                InitializeComponent();
            }

            private void Window_MouseDown(object sender, MouseButtonEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    DragMove();
            }

            private void CloseButtonHandler(object sender, RoutedEventArgs e)
            {
                Close();
            }


        private void LoginButtonHandler(object sender, RoutedEventArgs e)
        {
            login bejelntkezes = new login();
            bejelntkezes.Show();
            Close();
        }

        private void RegistrationButtonHandler(object sender, RoutedEventArgs e)
            {
                List<Users> users = new List<Users>();
                string userName = userBox.Text.ToString();
                string password = passBox.Password.ToString();
                string passwordAgain = passBox2.Password.ToString();




                if (userName.Equals(""))
                {
                    MessageBox.Show("Felhasználónév kötelező!");
                }
                else if (password.Equals("") || passwordAgain.Equals(""))
                {
                    MessageBox.Show("Jelszó kötelező!");
                }

                else if (!password.Equals(passwordAgain))
                {
                    MessageBox.Show("A jelszó nem egyezik!");
                }
                else
                {
                    login login1 = new login();
                    Users tmpUser = new Users(userName, password);

                    string json = File.ReadAllText("../../users.json");         //adat olvasása a fileból
                    users = JsonConvert.DeserializeObject<List<Users>>(json);   //a beolvasott adat konvertálása a user típusra

                    if (users.Contains(tmpUser))
                    {
                        MessageBox.Show("Már létezik ilyen felhasználó");
                    }
                    else
                    {
                        users.Add(new Users(userName, password));     //új felhasználó hozzá adása a már meglévő listához
                        File.WriteAllText("../../users.json", JsonConvert.SerializeObject(users));   //a lista írása a file-ba

                        MessageBox.Show("Sikeres regisztráció!");
                        login1.Show();

                        Close();

                    }
                }
            }
        }



        public class Users
        {
            public string usrName { get; set; }
            public string passWd { get; set; }


            public Users(String userName , String passWd)
            {
                this.usrName = userName;
                this.passWd = passWd;
            }

            public override bool Equals(object obj)
            {
                Users user = (Users)obj;
                if (this.usrName.Equals(user.usrName))
                {
                    return true;
                }
                return false;
            }
        }
    

    }





