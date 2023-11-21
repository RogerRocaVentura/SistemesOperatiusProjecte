using SuperSimpleTcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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

namespace Chess
{
    /// <summary>
    /// Lógica interna para WaitingRoom.xaml
    /// </summary>
    public partial class WaitingRoom : Window
    {
        public User user1 { get; set; }
        SimpleTcpClient client { get; set; }
        public bool gameHasStarted { get; set; }

        MainWindow main;
        public WaitingRoom(User user, SimpleTcpClient client, string usersOnline, MainWindow main)
        {
            InitializeComponent();
            this.main = main;
            gameHasStarted = false;
            this.client = client;
            user1 = user;            
            lstPlayerInfo.Items.Add(user1.name);
            lstPlayerInfo.Items.Add(user1.rating);
            playersList.MouseRightButtonUp += listBox1_MouseRightClick;


            

            // set events
            client.Events.Connected += Connected;
            client.Events.Disconnected += Disconnected;
            client.Events.DataReceived += DataReceived;
            

            System.Windows.Threading.DispatcherTimer loadingLabelTimer = new System.Windows.Threading.DispatcherTimer();
            loadingLabelTimer.Tick += new EventHandler(loadingLabelTimer_Tick);
            loadingLabelTimer.Interval = new TimeSpan(0, 0, 1);
            loadingLabelTimer.Start();

            // GETS USER LIST, RIGHT AFTER LOGIN IN
            string userOnlineFromSend = "";

            for (int i = 0; i < usersOnline.Length; i++)
            {
                if (usersOnline[i] != '-')
                {
                    userOnlineFromSend += usersOnline[i];
                }
                else
                {
                    if (userOnlineFromSend != "")
                    {
                        playersList.Items.Add(userOnlineFromSend);
                    }
                    userOnlineFromSend = "";
                }
            }
            // 

            

        }

        static void Connected(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine($"*** Server {e.IpPort} connected");
        }

        static void Disconnected(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine($"*** Server {e.IpPort} disconnected");
            MessageBox.Show("Disconnected");
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

        void GetData(string data, string userIP) ///////////////////////////////////////GET DATA //////////////////
        {   


            if (data.Contains("UpdateRatingR*"))
            {
                data = data.Split('*')[1];
                if (user1.login == data.Split('!')[0])
                {
                    user1.rating = Convert.ToInt32(data.Split('!')[1]);
                    lstPlayerInfo.Items.Clear();
                    lstPlayerInfo.Items.Add(user1.name);
                    lstPlayerInfo.Items.Add(user1.rating);
                }
            }
            
            if (data.Contains("challengeRejectR"))
            {
                NotFindingMatch();
            }
            if (data.Contains("watchR*"))
            {
                MessageBox.Show("watchR ok");
                data = data.Split('*')[1];
                //sqlName0!sqlName1#rating1_rating2
                string ratingsString = data.Split('#')[1];
                data = data.Split('#')[0];
                labelLoadingGame.Visibility = Visibility.Collapsed;
                User user2 = new User();
                User viewer = new User();

                string vName = data.Split('!')[0];
                //string vRating = data.Split('!')[1];

                viewer.name = vName;
                viewer.rating = Convert.ToInt32(ratingsString.Split('_')[0]);
                viewer.login = "v";
                
                string name2 = data.Split('!')[1];
                
                //string rating2 = data.Split('#')[0];
                

                user2.name = name2;
                user2.rating = Convert.ToInt32(ratingsString.Split('_')[1]);
                GameWindow game = new GameWindow(client, viewer, user2, true, this, true);
                this.Hide();
                game.Show();
                gameHasStarted = true;
            }

            if (data.Contains("challengeR*"))
            {
                NotFindingMatch();
                btnFindMatch.IsEnabled = false;
                string challengerName = data.Split('*')[1]; // Extract the challenger's name
                if (MessageBox.Show("The user " + challengerName + " is challenging you. Accept Challenge?", "Challenge", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    client.Send("challengeReject*" + challengerName); // Send the challenger's name

                    string rejectingUser = data.Split('*')[1]; // Extract the rejecting user's name
                    MessageBox.Show("The user " + rejectingUser + " has rejected your challenge.", "Challenge Rejected", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Any additional logic to handle the rejection, like re-enabling the 'Find Match' button
                    NotFindingMatch();
                }
                else
                {
                    client.Send("challengeAccept*" + challengerName); // Send the challenger's name
                    Wait(0.2);
                    NotFindingMatch();
                }
            }

            if (data.Contains("gameStart*"))
                {
                    labelLoadingGame.Visibility = Visibility.Collapsed;
                    User user2 = new User();

                    string[] splitData = data.Split('*')[1].Split('!');
                    string opponentName = splitData[0];
                    string assignedColor = splitData[1]; // Extract the assigned color

                    user2.name = opponentName;
                    //user2.rating = ...; // Get the opponent's rating
                    bool isPlayerWhite = assignedColor.Equals("white");

                    GameWindow game = new GameWindow(client, user1, user2, isPlayerWhite, this, false);
                    this.Hide();
                    game.Show();
                    gameHasStarted = true;
                }

            if (data.Contains("gameStart2*")) //JUST FOR TESTING // REMOVE LATER
            {
                
                labelLoadingGame.Visibility = Visibility.Collapsed;
                User user2 = new User();

                data = data.Split('*')[1];
                string name2 = data.Split('!')[0];
                data = data.Split('!')[1];
                string rating2 = data.Split('#')[0];
                data = data.Split('#')[1];                

                user2.name = name2;
                user2.rating = Convert.ToInt32(rating2);
                GameWindow game = new GameWindow(client, user1, user2, false, this, false); //////////////////////TESTING CHANGE COLOR;
                this.Hide();
                game.Show();
                gameHasStarted = true;
            }

            if (data.StartsWith("onlineUsers*list*"))
            {
                // Remove the identifier to parse only the list of users
                string usersData = data.Replace("onlineUsers*list*", "");
                // Split the users into an array, removing empty entries
                string[] users = usersData.Split(new[] { '!' }, StringSplitOptions.RemoveEmptyEntries);

                // Use the Dispatcher to update the UI on the UI thread
                Dispatcher.Invoke(() =>
                {
                    Console.WriteLine("Clearing players list"); // Debug log
                    playersList.Items.Clear();

                    // Add each user to the list, except the current user
                    foreach (var user in users)
                    {
                        Console.WriteLine($"Adding user to list: {user}"); // Debug log
                        if (!string.IsNullOrWhiteSpace(user) && user != user1.name)
                        {   
                            string[] users = usersData.Split(new[] { '!' }, StringSplitOptions.RemoveEmptyEntries);
                            Console.WriteLine($"Received users: {string.Join(", ", users)}"); // Debug log

                            playersList.Items.Add(user);
                        }
                    }
                    Console.WriteLine("Finished updating players list"); // Debug log
                });
            }
            
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            client.SendAsync("userQuit*");
            client.Disconnect();
            Application.Current.Shutdown();
        }
        private void listBox1_MouseRightClick(object sender, MouseButtonEventArgs e)
        {
            NotFindingMatch();
            btnFindMatch.IsEnabled = false;
            if (!gameHasStarted)
            {
                try
                {
                    if (playersList.SelectedItem != null && playersList.SelectedItem.ToString() != user1.name)
                    {
                        string selectedPlayer = playersList.SelectedItem.ToString();

                        if (MessageBox.Show("Do you want to challenge " + selectedPlayer + "?", "Challenge", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            // Challenge selected player
                            client.Send("challenge*" + selectedPlayer);
                            // Provide feedback to user that challenge has been sent
                            MessageBox.Show("Challenge sent to " + selectedPlayer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }


        private async void btnFindMatch_Click(object sender, RoutedEventArgs e)
        {
            user1.findingMatch = true;
            btnFindMatch.IsEnabled = false;
            await Task.Delay(1000);
            await client.SendAsync("findingMatch*");
            labelLoadingGame.Visibility = Visibility.Visible;
            btnStopFindMatch.Visibility = Visibility.Visible;
        }

        private async void btnStopFindMatch_Click(object sender, RoutedEventArgs e)
        {

            user1.findingMatch = false;
            btnFindMatch.IsEnabled = true;
            await Task.Delay(20);
            
            labelLoadingGame.Visibility = Visibility.Collapsed;
            btnStopFindMatch.Visibility = Visibility.Collapsed;
        }

        private void loadingLabelTimer_Tick(object sender, EventArgs e)
        {
            labelLoadingGame.Content += ". ";
            if (labelLoadingGame.Content.ToString() == "Finding Match . . . . ")
            {
                labelLoadingGame.Content = "Finding Match ";
            }
        }
        void NotFindingMatch()
        {
            client.Send("findingMatchStop*");
            btnFindMatch.IsEnabled = true;
            user1.findingMatch = false;
            btnStopFindMatch.Visibility = Visibility.Collapsed;
            labelLoadingGame.Visibility = Visibility.Collapsed;
            
        }
        public static void Wait(double seconds) //https://stackoverflow.com/questions/21547678/making-a-thread-wait-two-seconds-before-continuing-in-c-sharp-wpf
        {
            var frame = new DispatcherFrame();
            new Thread((ThreadStart)(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                frame.Continue = false;
            })).Start();
            Dispatcher.PushFrame(frame);
        }
        private void watchGame_click(object sender, MouseButtonEventArgs e)
        {            
            NotFindingMatch();
            
            if (!gameHasStarted)
            {
                try
                {
                    if (gamesList.SelectedItem != null)
                    {
                        string selectedGame = gamesList.SelectedItem.ToString();

                        if (MessageBox.Show("Do you want to watch " + selectedGame + "?", "Watch game", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            //do no stuff
                        }
                        else
                        {
                            //do yes stuff
                            string gameID = "";
                            for (int i = 0; i < selectedGame.Count(); i++)
                            {
                                if (selectedGame[i] != ' ')
                                {
                                    gameID += selectedGame[i];
                                }
                                else
                                {
                                    i = selectedGame.Count();
                                }
                            }                            
                            client.Send("watch*" + selectedGame);
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            main.Close();
        }
    }
}
