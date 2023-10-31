using SuperSimpleTcp;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
namespace Chess
{
    public partial class MainWindow : Window
    {
        User user1 { get; set; }
        User user2 { get; set; }
        bool player1Color { get; set; }
        public string hostIP_ { get; set; }        

        SimpleTcpClient client { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.Hide();

            user1 = new User();
            user2 = new User();
            player1Color = false;
            hostIP_ = "127.0.0.1";
            
            user1.name = null;

            GetHostIP get = new GetHostIP(this);
            get.Show();
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
            //Console.WriteLine($"[{e.IpPort}] {Encoding.UTF8.GetString(e.Data)}");
            string data = (Encoding.UTF8.GetString(e.Data));

            Dispatcher.Invoke(new Action(delegate
            {
                GetData(data, e.IpPort);
                //richTextBox1.Text += data;
            }));
        }

        void GetData(string data, string userIP)
        {
            if (data.Contains("loginReplyROk*"))
            {
                string usersOnline = data.Split('#')[1];
                data = data.Split('#')[0];
                data = data.Split('*')[1];
                user1.name = data.Split('!')[0];
                user1.rating = Convert.ToInt32(data.Split('!')[1]);
                WaitingRoom waitingRoom = new WaitingRoom(user1, client, usersOnline, this);
                Hide();
                waitingRoom.Show();
            }

            if (data.Contains("loginRFail*"))
            {
                loginBtn.IsEnabled = true;
            }

            if (data.Contains("userQuitR*"))
            {
                client.Disconnect();
                loginBtn.IsEnabled = true;
            }
        }

        private void createAcc_click(object sender, RoutedEventArgs e)
        {
            Hide();
            CreateAccount createAccount = new CreateAccount(this, hostIP_);
            createAccount.Show();
        }

        private async void login_click(object sender, RoutedEventArgs e)
        {
            if (txtLogin.Text.Length < 6 || txtLogin.Text.Length > 12 || txtPass.Text.Length < 6 || txtPass.Text.Length > 12)
            {
                MessageBox.Show("Login/Password must be between 6 to 12 characters");
                return;
            }

            loginBtn.IsEnabled = false;
            string hostIP = string.IsNullOrEmpty(hostIP_) ? "127.0.0.1" : hostIP_;
            string connectionStr = $"{hostIP}:5555";

            if (client == null)
            {
                client = new SimpleTcpClient(connectionStr);
                client.Events.Connected += Connected;
                client.Events.Disconnected += Disconnected;
                client.Events.DataReceived += DataReceived;
            }

            if (!client.IsConnected)
            {
                try
                {
                    client.Connect();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not connect to the server. Please try again later.");
                    loginBtn.IsEnabled = true;
                    return;
                }
            }

            try
            {
                await client.SendAsync("login*" + txtLogin.Text + "!" + txtPass.Text);
                await Task.Delay(1000); // Consider using a better synchronization strategy
                if (user1.name == null)
                {
                    client.Disconnect();
                    MessageBox.Show("Login failed. Please check your credentials and try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while attempting to log in. Please try again later.");
            }
            finally
            {
                loginBtn.IsEnabled = true;
            }
        }

        private void play_click(object sender, RoutedEventArgs e)
        {
            Hide();
            //GameWindow game = new GameWindow(this, user1);
            //game.Show();
        }

        static string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }

        private void txtLogin_changed(object sender, TextChangedEventArgs e)
        {
            if (!txtLogin.Text.All(chr => char.IsLetter(chr) || char.IsNumber(chr)))
            {
                MessageBox.Show("Special characters are prohibited, only letters and numbers allowed");
                txtLogin.Clear();
            }
        }

        private void txtPass_changed(object sender, TextChangedEventArgs e)
        {
            if (!txtPass.Text.All(chr => char.IsLetter(chr) || char.IsNumber(chr)))
            {
                MessageBox.Show("Special characters are prohibited, only letters and numbers allowed");
                txtPass.Clear();
            }
        }
    }
}
