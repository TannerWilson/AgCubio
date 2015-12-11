using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using Newtonsoft.Json;
using Network_Controller;
using System.Timers;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using MySql.Data.MySqlClient;
using System.IO;

namespace Server
{
    class Server
    {
        /// <summary>
        /// Game world
        /// </summary>
        static private World GameWorld;

        /// <summary>
        /// Clients connected to server
        /// </summary>
        static HashSet<StateObject> States;

        /// <summary>
        /// Heart beat of the game
        /// </summary>
        static private int TickRate;

        /// <summary>
        /// Heart beat of the game
        /// </summary>
        static System.Timers.Timer timer;

        /// <summary>
        /// Clients connected to the game
        /// </summary>
        static Dictionary<string, ClientState> Clients;

        /// <summary>
        /// Time when the server was spawned.
        /// </summary>
        public static int ServerStartTime;

        /// <summary>
        /// Total number of games played in the sever
        /// </summary>
        public static int CurrentGame;

        public static Stopwatch stopwatch = new Stopwatch();

        static void Main(string[] args)
        {
            start();
        }

        /// <summary>
        /// Populates the initial world (with food), 
        /// set up the heartbeat of the program, and await network client connections. 
        /// </summary>
        static public void start()
        {

            // Create world
            GameWorld = new World(1000, 1000);
            Network.Server_Awaiting_Client_Loop(PlayerConnect);
            Network.HTTPClientLoop(HTTPConnect);

            // Set up and start timer
            timer = new System.Timers.Timer(33);
            timer.Elapsed += new ElapsedEventHandler(Update);
            timer.Enabled = true;

            Clients = new Dictionary<string, ClientState>();
            CurrentGame = 0;

            // Create initial food
            lock (GameWorld)
            {
                GameWorld.CreateInitFood();
            }
            stopwatch.Start();
            Console.Read();
        }

        /// <summary>
        /// Recieve the player name, create player cube and send player string.
        /// </summary>
        /// <param name="State"></param>
        static void GetPlayerName(StateObject State)
        {
            string[] NewLineStrings = null;
            string PlayerName = null;
            string PlayerJsonString = null;

            // Process recieved string
            lock (State.sb)
            {
                NewLineStrings = State.sb.ToString().Split('\n');
                State.sb.Clear();
            }

            if (NewLineStrings != null)
            {
                PlayerName = NewLineStrings[0];
            }

            // Create player
            lock (GameWorld)
            {
                Console.WriteLine("Player " + PlayerName + " has entered the game!");
                PlayerJsonString = GameWorld.MakePlayer(PlayerName);

            }
            lock (Clients)
            {
                // Create player cube
                Cube PlayerCube = JsonConvert.DeserializeObject<Cube>(PlayerJsonString);

                HashSet<string> NewPlayerHash = new HashSet<string>();
                NewPlayerHash.Add(PlayerCube.uid);

                // Create and add new player client representation
                ClientState NewClient = new ClientState(State.workSocket, PlayerName, NewPlayerHash, PlayerCube.team_id);
                Clients.Add(PlayerName, NewClient);

                // Record the information needed for database purposes
                NewClient.UID = PlayerCube.uid;
                NewClient.TimeSpawned = stopwatch.ElapsedMilliseconds;
                NewClient.Game = CurrentGame++;
            }

            lock (State)
            {
                State.Name = PlayerName;
                State.ServerCallback = ClientData;
            }


            // Send palyer
            Network.Send(State.workSocket, PlayerJsonString + "\n");
            // Send food data
            SendInitFoodData(State);
            SendPlayersData(State);


            Network.GetData(State);
        }

        /// <summary>
        /// Send client the players
        /// </summary>
        /// <param name="State"></param>
        public static void SendPlayersData(StateObject State)
        {
            lock (GameWorld)
            {
                foreach (Cube PlayerCube in GameWorld.DictionaryOfCubes.Values)
                {
                    string JsonCube = JsonConvert.SerializeObject(PlayerCube);
                    Network.Send(State.workSocket, JsonCube + "\n");
                }
            }
        }

        /// <summary>
        /// Sends initial food
        /// </summary>
        /// <param name="State"></param>
        public static void SendInitFoodData(StateObject State)
        {
            lock (GameWorld)
            {
                foreach (Cube FoodCube in GameWorld.FoodCubes.Values)
                {
                    string JsonCube = JsonConvert.SerializeObject(FoodCube);
                    Network.Send(State.workSocket, JsonCube + "\n");
                }
            }
        }
        /// <summary>
        /// Receives data from clients. Should be either (move,x,y) or (split,x,y). 
        /// </summary>
        /// <param name="request"></param>
        static public void ClientData(StateObject State)
        {
            lock (State)
            {
                string[] SplitStrings = null;
                string ActionRequest = null;
                // Process data recieved
                lock (State.sb)
                {
                    SplitStrings = State.sb.ToString().Split('\n');
                    State.sb.Clear();

                    // Loop throgh each request
                    foreach (string Req in SplitStrings)
                    {
                        ActionRequest = Req;
                        ActionRequest = ActionRequest.TrimStart('(');
                        ActionRequest = ActionRequest.TrimEnd(')');

                        string[] RequestSplitString = ActionRequest.Split(',');

                        //Do Action
                        lock (GameWorld)
                        {

                            if (RequestSplitString.Length == 3)
                            {

                                // Ensure the request is not empty
                                if (RequestSplitString[0] != "" && !RequestSplitString[0].StartsWith("/0/0/0"))
                                {
                                    // Ensure request is formatted correctly
                                    int x, y;
                                    if (Int32.TryParse(RequestSplitString[1].ToString(), out x) && Int32.TryParse(RequestSplitString[2].ToString(), out y))
                                    {

                                        ClientState PlayerState = Clients[State.Name];

                                        List<string> ToAdd = new List<string>();

                                        // Set merge timer
                                        if (RequestSplitString[0] == "split")
                                        {
                                            PlayerState.timer += 10;
                                        }
                                        // Add new cubes to the player's set
                                        foreach (string UID in PlayerState.UIDS)
                                        {
                                            string ReturnUID = GameWorld.ActionCommand(RequestSplitString[0], x, y, UID);
                                            if (ReturnUID != null && !PlayerState.UIDS.Contains(ReturnUID))
                                                ToAdd.Add(ReturnUID);
                                        }

                                        foreach (string NewCubeUID in ToAdd)
                                        {
                                            PlayerState.UIDS.Add(NewCubeUID);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Inalid request recieved.");
                                    }
                                }
                            }
                            else
                            {
                                State.sb.Append(Req);
                            }
                        }
                    }
                }
                // Ask for more data after processing all previous requests
                Network.GetData(State);
            }
        }

        /// <summary>
        /// Called when a player hits a virus
        /// </summary>
        /// <param name="cube"></param>
        /// <returns></returns>
        public static string VirusSplit(Cube cube)
        {
            ClientState PlayerState = Clients[cube.Name];

            List<string> ToAdd = new List<string>();
            PlayerState.timer += 10;
            foreach (string UID in PlayerState.UIDS)
            {
                string ReturnUID = GameWorld.ActionCommand("split", (int)cube.loc_x, (int)cube.loc_y, UID);
                if (ReturnUID != null && !PlayerState.UIDS.Contains(ReturnUID))
                    ToAdd.Add(ReturnUID);
            }

            foreach (string NewCubeUID in ToAdd)
            {
                PlayerState.UIDS.Add(NewCubeUID);
            }
            return GameWorld.GenerateNewVirus();
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Merge()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="State"></param>
        static void PlayerConnect(StateObject State)
        {
            State.ServerCallback = GetPlayerName;
            Network.GetData(State);
        }

        /// <summary>
        /// Connect to the web page server
        /// </summary>
        /// <param name="State"></param>
        static void HTTPConnect(StateObject State)
        {
            State.ServerCallback = HTTPGetCommandRecieve;
            Network.GetData(State);
        }

        /// <summary>
        /// Receives the requests from the web page.
        /// </summary>
        /// <param name="State"></param>
        static void HTTPGetCommandRecieve(StateObject State)
        {
            string[] SplitHTTPString = null;
            lock (State.sb)
            {
                string[] stringSeparators = new string[] { "\r\n" };
                SplitHTTPString = State.sb.ToString().Split(stringSeparators, StringSplitOptions.None);
            }

            string[] SplitGetString = SplitHTTPString[0].Split(' ');
            HTTPRoute(SplitGetString[1], State);

        }

        /// <summary>
        /// Processes the requests from the web page.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="State"></param>
        static void HTTPRoute(string Route, StateObject State)
        {
            Console.WriteLine(Route);
            string ArgumentName = null;
            string Argument = null;

            if(Route.Contains("?"))
            {
                try
                {
                    string[] SplitString = Route.Split('?');

                    Route = SplitString[0];

                    string[] ArgumentSplit = SplitString[1].Split('=');

                    ArgumentName = ArgumentSplit[0];

                    Argument = ArgumentSplit[1];
                }
                catch(Exception e)
                {
                    
                }

            }
            switch (Route)
            {
                case "/":
                    // Generate base page
                    StringBuilder sb = new StringBuilder();
                    sb.Append(ReadHTML("top.html"));
                    //Gen Middle here
                    sb.Append(ReadHTML("bottom.html"));
                    HTTPReturn(State, sb.ToString());
                    break;

                case "/highscores":
                    // Fetch top socres
                    String PlayerScores = GetScores();

                    // Generate Score page
                    sb = new StringBuilder();
                    sb.Append(ReadHTML("top.html"));
                    sb.Append(GenerateHighScoreHTMLTable(PlayerScores));
                    sb.Append(ReadHTML("bottom.html"));
                    HTTPReturn(State, sb.ToString());
                    break;

                case "/scores":
                    // Fetch players
                    String Players = GetPlayers();

                    // Generate player page
                    sb = new StringBuilder();
                    sb.Append(ReadHTML("top.html"));
                    //Gen Middle here
                    sb.Append(GeneratePlayerHTMLTable(Players));
                    sb.Append(ReadHTML("bottom.html"));
                    HTTPReturn(State, sb.ToString());
                    break;
                case "/games":
                    string PlayerInfo = GetPlayerData(Argument);
                    // Generate player page
                    sb = new StringBuilder();
                    sb.Append(ReadHTML("top.html"));
                    //Gen Middle here
                    sb.Append(GeneratePlayerHTMLTable(PlayerInfo));
                    sb.Append(ReadHTML("bottom.html"));
                    HTTPReturn(State, sb.ToString());
                    break;
                case "/eaten":
                    string GameInfo = GetGameIDPlayers(Argument);
                    // Generate player page
                    sb = new StringBuilder();
                    sb.Append(ReadHTML("top.html"));
                    //Gen Middle here
                    sb.Append(GeneratePlayerHTMLTable(GameInfo));
                    sb.Append(ReadHTML("bottom.html"));
                    HTTPReturn(State, sb.ToString());
                    break;
                default:
                    // Generate player page
                    sb = new StringBuilder();
                    sb.Append(ReadHTML("top.html"));
                    //Gen Middle here
                    sb.Append("404!");
                    sb.Append(ReadHTML("bottom.html"));
                    HTTPReturn(State, sb.ToString());
                    break;
            }
        }

        public static string GetPlayerData(string PlayerName)
        {
            const string connectionString = "server=atr.eng.utah.edu;database=cs3500_tannerw;uid=cs3500_tannerw;password=PSWRD";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Open a connection to tour database
                conn.Open();

                // Select all from the score table and sort
                MySqlCommand Command = conn.CreateCommand();
                Command.CommandText = "SELECT * FROM Player_Info WHERE Name = @PlayerName;";
                Command.Prepare();

                Command.Parameters.AddWithValue("@PlayerName", PlayerName);

                StringBuilder Players = new StringBuilder();
                using (MySqlDataReader reader = Command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Players.Append(reader["Name"].ToString() + "," + reader["MaxMass"].ToString() + "," + reader["UID"].ToString() + ","
                            + reader["LifeTime"].ToString() + "," + reader["Kills"].ToString() + ", " + reader["ListOfKills"].ToString() + ","
                            + reader["TimeOfDeath"].ToString() + "," + reader["GameID"] + "\n");
                    }
                }
                return Players.ToString();
            }
        }

        public static string GetGameIDPlayers(string GameId)
        {
            const string connectionString = "server=atr.eng.utah.edu;database=cs3500_tannerw;uid=cs3500_tannerw;password=PSWRD";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Open a connection to tour database
                conn.Open();

                // Select all from the score table and sort
                MySqlCommand Command = conn.CreateCommand();
                Command.CommandText = "SELECT * FROM Player_Info WHERE GameID = @GameID;";
                Command.Prepare();

                Command.Parameters.AddWithValue("@GameID", GameId);

                StringBuilder Players = new StringBuilder();
                using (MySqlDataReader reader = Command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Players.Append(reader["Name"].ToString() + "," + reader["MaxMass"].ToString() + "," + reader["UID"].ToString() + ","
                            + reader["LifeTime"].ToString() + "," + reader["Kills"].ToString() + ", " + reader["ListOfKills"].ToString() + ","
                            + reader["TimeOfDeath"].ToString() + "," + reader["GameID"] + "\n");
                    }
                }
                return Players.ToString();
            }
        }

        /// <summary>
        /// Gets the top five scores from the database
        /// </summary>
        /// <returns></returns>
        public static string GetScores()
        {
            const string connectionString = "server=atr.eng.utah.edu;database=cs3500_tannerw;uid=cs3500_tannerw;password=PSWRD";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Open a connection to tour database
                conn.Open();

                // Select all from the score table and sort
                MySqlCommand ExistCommand = conn.CreateCommand();
                ExistCommand.CommandText = "SELECT * FROM Score_Table ORDER BY Mass DESC;";

                // Read top 5 entries
                int count = 0;
                string Scores = "";
                using (MySqlDataReader reader = ExistCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Scores += reader["Name"].ToString() + "," + reader["Mass"].ToString() + "," + reader["UID"].ToString() + "\n";
                        count++;
                        if (count > 4)
                            break;
                    }
                }
                return Scores;
            }
        }

        /// <summary>
        /// Returns a string of player strings
        /// </summary>
        /// <returns></returns>
        public static string GetPlayers()
        {
            const string connectionString = "server=atr.eng.utah.edu;database=cs3500_tannerw;uid=cs3500_tannerw;password=PSWRD";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Open a connection to tour database
                conn.Open();

                // Select all from the score table and sort
                MySqlCommand ExistCommand = conn.CreateCommand();
                ExistCommand.CommandText = "SELECT * FROM Player_Info;";

                // Read top 5 entries

                StringBuilder Players = new StringBuilder();
                using (MySqlDataReader reader = ExistCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Players.Append(reader["Name"].ToString() + "," + reader["MaxMass"].ToString() + "," + reader["UID"].ToString() + ","
                            + reader["LifeTime"].ToString() + "," + reader["Kills"].ToString() + ", " + reader["ListOfKills"].ToString() + ","
                            + reader["TimeOfDeath"].ToString()+ ","+ reader["GameID"] + "\n");
                    }
                }
                return Players.ToString();
            }
        }

        /// <summary>
        /// Create the HTML table for high scores
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GenerateHighScoreHTMLTable(string data)
        {
            StringBuilder HTML = new StringBuilder();
            HTML.Append("<table border=\"1\" style=\"width:50%\">");
            HTML.Append("<tr><td>Name</td><td>Mass</td><td>UID</td></tr>");

            string[] DataString = data.Split('\n');
            // Create  a row for each player in the data string
            foreach (string PlayerDataString in DataString)
            {
                if (PlayerDataString != "")  // Ensure valid string
                {
                    string[] DataItems = PlayerDataString.Split(',');
                    // Generate row with player data
                    HTML.Append("<tr><td><a href =\"/games?player=" + DataItems[0] + "\">" + DataItems[0] + "</a></td><td>" + DataItems[1] + "</td><td>" + DataItems[2] + "</td></tr>");
                }
            }
            HTML.Append("</table>");
            return HTML.ToString();
        }


        /// <summary>
        /// Create the HTML table for high scores
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GeneratePlayerHTMLTable(string data)
        {
            StringBuilder HTML = new StringBuilder();
            // Set up table start and top row
            HTML.Append("<table border=\"1\" style=\"width:50%\">");
            HTML.Append("<tr><td>Name</td><td>MaxMass</td><td>UID</td><td>LifeTime</td><td>Kills</td><td>ListOfKills</td><td>TimeOfDeath</td><td>GameID</td></tr>");

            string[] DataString = data.Split('\n');
            // Create  a row for each player in the data string
            foreach (string PlayerDataString in DataString)
            {
                if (PlayerDataString != "") // Ensure valid string
                {
                    string[] DataItems = PlayerDataString.Split(',');

                    // Geterate String from list of kills
                    string[] PlayerKills = DataItems[5].Split('$');
                    string kills = "";
                    foreach (string player in PlayerKills)
                    {
                        kills += player + ", ";
                    }
                    // Generate row with player data
                    HTML.Append("<tr><td><a href =\"/games?player=" + DataItems[0] + "\">" + DataItems[0] + "</a></td><td>" + DataItems[1] + "</td><td>" + DataItems[2] + "</td><td>" + DataItems[3] + "</td><td>" + DataItems[4] + "</td><td>" + kills + "</td><td>" + DataItems[6] + "</td><td>"+ DataItems[7]+ "</td></tr>");
                }
            }
            HTML.Append("</table>");
            return HTML.ToString();
        }


        /// <summary>
        /// Send the HTML to the web page
        /// </summary>
        /// <param name="State"></param>
        /// <param name="html"></param>
        static void HTTPReturn(StateObject State, string html)
        {
            State.ServerCallback = HTTPSendDone;
            Network.HTTPSend(State, "HTTP/1.1 200 OK\r\nConnection: close\r\nContent-Type: text/html; charset=UTF-8\r\n\r\n" + html);
        }

        /// <summary>
        ///  Called after for a connection is made.
        /// </summary>
        /// <param name="State"></param>
        public static void HTTPSendDone(StateObject State)
        {

            Console.WriteLine("END");
            State.workSocket.Shutdown(SocketShutdown.Send);
        }

        /// <summary>
        /// Reads HTML files
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static string ReadHTML(string Path)
        {
            return File.ReadAllText(Path);
        }

        static void SendHighScoreToDataBase(ClientState Client)
        {
            const string connectionString = "server=atr.eng.utah.edu;database=cs3500_tannerw;uid=cs3500_tannerw;password=PSWRD";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Open a connection
                conn.Open();

                // Create a command
                MySqlCommand command = conn.CreateCommand();

                command.CommandText = "INSERT INTO `cs3500_tannerw`.`Score_Table` (`Name`, `Mass`, `UID`) VALUES (@Name, @MaxMass, @UID);";
                command.Prepare();

                command.Parameters.AddWithValue("@MaxMass", Client.Mass);
                command.Parameters.AddWithValue("@UID", Client.UID);
                command.Parameters.AddWithValue("@Name", Client.Name);


                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Record player information to the SQL database
        /// </summary>
        /// <param name="Client"></param>
        static void SendToDataBase(ClientState Client)
        {
            const string connectionString = "server=atr.eng.utah.edu;database=cs3500_tannerw;uid=cs3500_tannerw;password=PSWRD";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {


                // Open a connection
                conn.Open();


                // Create a command
                MySqlCommand command = conn.CreateCommand();

                command.CommandText = "INSERT INTO `cs3500_tannerw`.`Player_Info` (`LifeTime`, `MaxMass`, `HighestRank`, `Kills`, `ListOfKills`, `TimeOfDeath`, `UID`, `Name`,`GameID`) VALUES(@LifeTime, @MaxMass, @HighestRank, @Kills, @ListOfKills, @TimeOfDeath, @UID,@Name,@GameID);";
                command.Prepare();

                command.Parameters.AddWithValue("@LifeTime", Client.GetLifeTime().ToString());
                command.Parameters.AddWithValue("@MaxMass", Client.Mass);
                command.Parameters.AddWithValue("@HighestRank", 1);
                command.Parameters.AddWithValue("@Kills", Client.kills.ToString());
                command.Parameters.AddWithValue("@ListOfKills", Client.PlayersKilledDataString());
                command.Parameters.AddWithValue("@TimeOfDeath", Client.GetDeathTime());
                command.Parameters.AddWithValue("@UID", Client.UID);
                command.Parameters.AddWithValue("@Name", Client.Name);
                command.Parameters.AddWithValue("@GameID", Client.Game);

                //For Debuging
                //command.Parameters.AddWithValue("@LifeTime", "12");
                //command.Parameters.AddWithValue("@MaxMass", 12);
                //command.Parameters.AddWithValue("@HighestRank", 1);
                //command.Parameters.AddWithValue("@Kills", 12);
                //command.Parameters.AddWithValue("@ListOfKills", "Your Dad");
                //command.Parameters.AddWithValue("@TimeOfDeath", 120);
                //command.Parameters.AddWithValue("@UID", 8008);
                //command.Parameters.AddWithValue("@Name", "Test");
                //command.Parameters.AddWithValue("@GameID", 8008);


                command.ExecuteNonQuery();
            }
        }




        /// <summary>
        /// Updates all aspects of the world.
        /// </summary>
        static public void Update(object source, ElapsedEventArgs e)
        {
            lock (GameWorld)
            {
                List<Cube> ToDelete = new List<Cube>();

                List<string> UpdatedCubes = new List<string>();

                // Create new food cube and send to each client
                string FoodCube = GameWorld.GenerateNewFood();

                // Send new food cube to clients
                foreach (ClientState state in Clients.Values)
                {
                    if (FoodCube != null)
                        Network.Send(state.Socket, FoodCube);
                }

                // Check for changes
                foreach (Cube CubeItem in GameWorld.DictionaryOfCubes.Values)
                {
                    string UpdatedJsonCube = CubeItem.Update();
                    // If updated, add to updated list
                    if (UpdatedJsonCube != null)
                    {
                        UpdatedCubes.Add(UpdatedJsonCube);
                        Cube SQLServerCube = JsonConvert.DeserializeObject<Cube>(UpdatedJsonCube);
                        float OldMass = Clients[SQLServerCube.Name].Mass;
                        if (OldMass < SQLServerCube.Mass)
                            Clients[SQLServerCube.Name].Mass = SQLServerCube.Mass;
                    }

                    //Check if any of the client cubes collide with food
                    foreach (Cube FCube in GameWorld.FoodCubes.Values)
                    {
                        // Remove food
                        if (Cube.Collide(CubeItem, FCube))
                        {
                            if (FCube.Virus)
                            {
                                //string virus = VirusSplit(CubeItem);
                                //UpdatedCubes.Add(virus);
                                //ToDelete.Add(FCube);
                            }

                            // Update food and player masses to be sent to clients
                            FCube.Mass = 0;
                            ToDelete.Add(FCube);
                            CubeItem.Mass += 3;
                        }
                    }

                    // Check for player collisions
                    foreach (Cube Player in GameWorld.DictionaryOfCubes.Values)
                    {
                        // Colided with a player
                        if (Cube.Collide(CubeItem, Player))
                        {
                            // Colided with player not on our team
                            if (CubeItem.team_id != Player.team_id)
                            {
                                // Check for lower massed cube
                                if (CubeItem.Mass > (Player.Mass + 30) && (CubeItem.Mass != 0 && Player.Mass != 0))
                                {
                                    // Add player to clients kills
                                    Clients[CubeItem.Name].kills++;
                                    Clients[CubeItem.Name].PlayersKilled.Add(Player.Name);
                                    Player.Mass = 0;
                                    // Add lower mass cube to deleted list
                                    ToDelete.Add(Player);
                                }
                            }
                        }
                    }
                }

                // Send all updated data to clients
                foreach (ClientState state in Clients.Values)
                {
                    // Remove splitted player cubes
                    //Cube first = GameWorld.DictionaryOfCubes[state.UIDS.First()];
                    //List<string> UIDSDelete = new List<string>();
                    //foreach (string uid in state.UIDS)
                    //{
                    //    if (uid != first.uid)
                    //    {
                    //        Cube next = GameWorld.DictionaryOfCubes[uid];
                    //        first.Mass += next.Mass;
                    //        GameWorld.DictionaryOfCubes.Remove(uid);
                    //        UIDSDelete.Add(uid);
                    //    }
                    //}

                    //// remove 
                    //foreach (string uid in UIDSDelete)
                    //{
                    //    state.UIDS.Remove(uid);
                    //}

                    // Send changed cube values
                    foreach (string UpdatedCubeItem in UpdatedCubes)
                    {
                        Network.Send(state.Socket, UpdatedCubeItem);
                    }

                    // Send all items to be deleted
                    foreach (Cube item in ToDelete)
                    {
                        string CubeString = JsonConvert.SerializeObject(item);
                        Network.Send(state.Socket, CubeString + '\n');
                    }

                    // Delete all items 
                    foreach (Cube item in ToDelete)
                    {
                        // Remove cubes from server's data structures

                        if (GameWorld.DictionaryOfCubes.ContainsKey(item.uid))
                            GameWorld.DictionaryOfCubes.Remove(item.uid);

                        if (GameWorld.FoodCubes.ContainsKey(item.uid))
                            GameWorld.FoodCubes.Remove(item.uid);

                        string CubeName = item.Name;
                        if (CubeName != "")
                        {
                            // Delete Players that have been merged
                            ClientState CurrentClient = Clients[CubeName];

                            if (CurrentClient != null && CurrentClient.UIDS.Contains(item.uid))
                            {
                                CurrentClient.UIDS.Remove(item.uid);

                                if (CurrentClient.UIDS.Count < 1)
                                {
                                    CurrentClient.TimeDied = stopwatch.ElapsedMilliseconds;
                                    CurrentClient.Socket.Close();
                                    SendToDataBase(CurrentClient);
                                    SendHighScoreToDataBase(CurrentClient);
                                    //Clients.Remove(CurrentClient.Name);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Object used to represent each client connected to the server.
        /// </summary>
        protected class ClientState
        {
            /// <summary>
            /// The timer used for merging
            /// </summary>
            public int timer;

            /// <summary>
            /// Socket used to send information to clients
            /// </summary>
            public Socket Socket;

            /// <summary>
            /// Players name
            /// </summary>
            public string Name;

            /// <summary>
            /// UID's of the stings belonging to this client
            /// </summary>
            public HashSet<string> UIDS;

            /// <summary>
            /// Team that the cubes belong to 
            /// </summary>
            public int TeamID;

            /// <summary>
            /// Number of kills this client has scored
            /// </summary>
            public int kills;

            /// <summary>
            /// List of players killed/eaten
            /// </summary>
            public HashSet<string> PlayersKilled;

            /// <summary>
            /// Time the client connected
            /// </summary>
            public long TimeSpawned;

            /// <summary>
            /// Time the player disconnected/died
            /// </summary>
            public long TimeDied;

            /// <summary>
            /// Client player's mass
            /// </summary>
            public float Mass;

            /// <summary>
            /// UID of the first cube to connect from this client
            /// </summary>
            public string UID;

            /// <summary>
            /// This players game number
            /// </summary>
            public int Game;

            public ClientState(Socket _socket, string _name, HashSet<string> uid, int team)
            {
                Socket = _socket;
                Name = _name;
                UIDS = uid;
                TeamID = team;
                timer = 0;
                PlayersKilled = new HashSet<string>();
            }

            public int GetLifeTime()
            {
                return Convert.ToInt32((TimeDied / 1000) - (TimeSpawned / 1000));
            }

            public int GetDeathTime()
            {
                return Convert.ToInt32(TimeDied / 1000);
            }



            public string PlayersKilledDataString()
            {
                StringBuilder PlayersKilledStringBuilder = new StringBuilder();
                if (PlayersKilled.Count > 0)
                {

                    foreach (string Item in PlayersKilled)
                    {
                        PlayersKilledStringBuilder.Append(Item + "$");
                    }
                }
                else
                    return "NONE";

                return PlayersKilledStringBuilder.ToString();
            }
        }
    }
}
