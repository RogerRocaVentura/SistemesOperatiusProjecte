using SuperSimpleTcp;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    public partial class CreateAccount : Window
    {
        MainWindow main;
        SimpleTcpClient client;

        public CreateAccount(MainWindow mainWindow, string hostIP)
        {
            InitializeComponent();
            main = mainWindow;
            
            string connectionStr = $"{hostIP}:5555";
            client = new SimpleTcpClient(connectionStr);

            client.Events.Connected += Connected;
            client.Events.Disconnected += Disconnected;
            client.Events.DataReceived += DataReceived;

            try
            {
                if (!client.IsConnected)
                {
                    client.Connect();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Can't connect to server: {ex.Message}");
            }
        }

        void Limpar()
        {
            txtName.Text = "";
            txtPass.Text = "";
            txtPassConf.Text = "";
            txtUser.Text = "";
        }

        static void Connected(object sender, ConnectionEventArgs e)
        {
            //Console.WriteLine($"*** Server {e.IpPort} connected");
        }

        static void Disconnected(object sender, ConnectionEventArgs e)
        {
            //Console.WriteLine($"*** Server {e.IpPort} disconnected");
            //MessageBox.Show("Disconnected");
        }

        void DataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Data);
            Dispatcher.Invoke(() => { GetData(data, e.IpPort); });
        }

        void GetData(string data, string userIP)
        {
            if (data.Contains("createAccountSuccessR*"))
            {
                MessageBox.Show("Success!");
                Limpar();
                Close();
                main.Show();
            }
            if (data.Contains("createAccountFailR*"))
            {
                MessageBox.Show(data.Split('*')[1]);
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (txtPass.Text == "" || txtPassConf.Text == "" || txtUser.Text == "")
            {
                MessageBox.Show("Fill all the fields to register");
                Limpar();
            }
            else if (txtPass.Text == txtPassConf.Text)
            {
                if (txtPass.Text.Length > 5 && txtPass.Text.Length < 15 && txtUser.Text.Length > 5 && txtUser.Text.Length < 15)
                {
                    //client.Send($"createAccount*{txtName.Text}!{txtUser.Text}#{txtPass.Text}");
                    client.Send($"createAccount*{txtUser.Text}!{txtPass.Text}!{txtName.Text}");

                }
                else
                {
                    MessageBox.Show("ERROR: Login/Password must have between 6 and 14 characters!");
                    txtUser.Focus();
                    txtPass.Focus();
                }                
            }
            else
            {
                MessageBox.Show("Passwords don't match!");
                txtPass.Text = "";
                txtPassConf.Text = "";
                txtPass.Focus();
            }
        }

        private void txtName_Changed(object sender, TextChangedEventArgs e)
        {
            if (txtName.Text.All(chr => char.IsLetter(chr) || char.IsNumber(chr)))
            {

            }
            else
            {
                MessageBox.Show("Special characters are prohibited, only letters and numbers allowed");
                txtName.Clear();
            }
        }

        private void txtUser_Changed(object sender, TextChangedEventArgs e)
        {
            if (txtUser.Text.All(chr => char.IsLetter(chr) || char.IsNumber(chr)))
            {

            }
            else
            {
                MessageBox.Show("Special characters are prohibited, only letters and numbers allowed");
                txtUser.Clear();
            }
        }

        private void txtPass_Changed(object sender, TextChangedEventArgs e)
        {
            if (txtPass.Text.All(chr => char.IsLetter(chr) || char.IsNumber(chr)))
            {

            }
            else
            {
                MessageBox.Show("Special characters are prohibited, only letters and numbers allowed");
                txtPass.Clear();
            }
        }

        private void txtPass_Changed2(object sender, TextChangedEventArgs e)
        {
            if (txtPassConf.Text.All(chr => char.IsLetter(chr) || char.IsNumber(chr)))
            {

            }
            else
            {
                MessageBox.Show("Special characters are prohibited, only letters and numbers allowed");
                txtPassConf.Clear();
            }
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            main.Show();
        }
    }
}
