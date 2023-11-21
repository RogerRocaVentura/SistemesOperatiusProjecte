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
        public event EventHandler<bool> LoginProcessed;

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
            Console.WriteLine($"*** Server {e.IpPort} connected");
        }

        static void Disconnected(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine($"*** Server {e.IpPort} disconnected");
            //MessageBox.Show("Disconnected");
        }
        void DataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Data);
            Console.WriteLine($"Data received: {data}");

            Dispatcher.Invoke(() =>
            {
                try
                {
                    if (data.Contains("loginReplyROk*"))
                    {
                        // Process successful login
                        Console.WriteLine("Login successful.");
                        OpenWaitingRoom(); // Make sure to implement this method
                    }
                    else if (data.Contains("loginRFail*"))
                    {
                        // Process failed login
                        MessageBox.Show("Login failed. Please check your credentials and try again.");
                        loginBtn.IsEnabled = true;
                    }
                    else if (data.Contains("usersOnlineBroadcast*"))
                    {
                        // Process the list of online users
                        //UpdateOnlineUsersList(data);
                    }
                    // ... other data handling if necessary
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while processing server response: {ex.Message}");
                }
            });
        }



        private void createAcc_click(object sender, RoutedEventArgs e)
        {
            Hide();
            CreateAccount createAccount = new CreateAccount(this, hostIP_);
            createAccount.Show();
        }

        private TaskCompletionSource<bool> loginCompletionSource;

        private void OpenWaitingRoom()
        {
            try
            {
                Console.WriteLine("Opening WaitingRoom window...");
                WaitingRoom waitingRoom = new WaitingRoom(user1, client, "", this); // Pass the correct usersOnline if needed
                waitingRoom.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open WaitingRoom window: {ex.Message}");
            }
        }

        

        private async void login_click(object sender, RoutedEventArgs e)
        {
            // Validate the input length first.
            if (txtLogin.Text.Length < 6 || txtLogin.Text.Length > 12 || txtPass.Text.Length < 6 || txtPass.Text.Length > 12)
            {
                MessageBox.Show("Login/Password must be between 6 to 12 characters");
                return;
            }

            // Disable the login button to prevent multiple login attempts.
            loginBtn.IsEnabled = false;
            string hostIP = string.IsNullOrEmpty(hostIP_) ? "127.0.0.1" : hostIP_;
            string connectionStr = $"{hostIP}:5555";

            // Only create a new client if one does not already exist.
            if (client == null)
            {
                client = new SimpleTcpClient(connectionStr);
                client.Events.Connected += Connected;
                client.Events.Disconnected += Disconnected;
                client.Events.DataReceived += DataReceived;
            }

            // Attempt to connect to the server if not already connected.
            if (!client.IsConnected)
            {
                try
                {
                    client.Connect();
                    Console.WriteLine("Connected to server.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not connect to the server: {ex.Message}");
                    loginBtn.IsEnabled = true; // Re-enable the login button on failure.
                    return;
                }
            }

            // Initialize the TaskCompletionSource
            loginCompletionSource = new TaskCompletionSource<bool>();

            // Send login credentials to the server.
            try
            {
                client.Send($"login*{txtLogin.Text}!{txtPass.Text}");
                Console.WriteLine("Login credentials sent.");

                // Wait for the DataReceived event to process the server response.
                bool loginSuccessful = await loginCompletionSource.Task;

                if (loginSuccessful)
                {
                    // Login was successful, proceed to open the new window or next steps in the application.
                    // For example, OpenMainWindow(); or similar code to transition to the next view.
                    Console.WriteLine("Login successful, opening main window.");
                }
                else
                {
                    // Login failed, inform the user.
                    MessageBox.Show("Login failed. Please check your credentials and try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while attempting to log in: {ex.Message}");
            }
            finally
            {
                // Re-enable the login button after the attempt for both success and failure.
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
