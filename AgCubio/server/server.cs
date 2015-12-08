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
using MySql.Data.MySqlClient;

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
        static Timer timer;

        /// <summary>
        /// Clients connected to the game
        /// </summary>
        static Dictionary<string, ClientState> Clients;

        /// <summary>
        /// Time when the server was spawned.
        /// </summary>
        public static int ServerStartTime;

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

            // Set up and start timer
            timer = new Timer(33);
            timer.Elapsed += new ElapsedEventHandler(Update);
            timer.Enabled = true;

            Clients = new Dictionary<string, ClientState>();

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
                Cube PlayerCube = JsonConvert.DeserializeObject<Cube>(PlayerJsonString);

                HashSet<string> NewPlayerHash = new HashSet<string>();
                NewPlayerHash.Add(PlayerCube.uid);

                ClientState NewClient = new ClientState(State.workSocket, PlayerName, NewPlayerHash, PlayerCube.team_id);
                Clients.Add(PlayerName, NewClient);
                NewClient.TimeSpawned = stopwatch.ElapsedMilliseconds;
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

        static void PlayerConnect(StateObject State)
        {
            State.ServerCallback = GetPlayerName;
            Network.GetData(State);
        }


        static void SendToDataBase(ClientState Client)
        {
            const string connectionString = "server=atr.eng.utah.edu;database=cs3500_tannerw;uid=cs3500_tannerw;password=PSWRD";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {


                    // Open a connection
                    conn.Open();

                    // Create a command
                    MySqlCommand command = conn.CreateCommand();

                    command.CommandText = "INSERT INTO `cs3500_tannerw`.`Player_Info` (`LifeTime`, `MaxMass`, `HighestRank`, `Kills`, `ListOfKills`, `TimeOfDeath`, `UID`) VALUES(@LifeTime, @MaxMass, @HighestRank, @Kills, @ListOfKills, @TimeOfDeath, @UID);";
                    command.Prepare();

                    command.Parameters.AddWithValue("@LifeTime", Client.GetLifeTime().ToString());
                    command.Parameters.AddWithValue("@MaxMass", Client.Mass);
                    command.Parameters.AddWithValue("@HighestRank", 1);
                    command.Parameters.AddWithValue("@Kills", Client.kills.ToString());
                    command.Parameters.AddWithValue("@ListOfKills", Client.PlayersKilled.ToString());
                    command.Parameters.AddWithValue("@TimeOfDeath", Client.TimeDied.ToString());
                    command.Parameters.AddWithValue("@UID", Client.UIDS.First());

                    command.ExecuteNonQuery();
                } catch(Exception e)
                {
                    throw e;
                    //UPDATE `cs3500_tannerw`.`Player_Info` SET `LifeTime`='1233', `MaxMass`='123', `HighestRank`='123', `Kills`='79', `ListOfKills`='Your Mom2', `TimeOfDeath`='1000111' WHERE `UID`='8008';
                }



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
                                string virus = VirusSplit(CubeItem);
                                UpdatedCubes.Add(virus);
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
                                Clients.Remove(CurrentClient.Name);
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
            public int timer;
            public Socket Socket;
            public string Name;
            public HashSet<string> UIDS;
            public int TeamID;
            public int kills;
            public HashSet<string> PlayersKilled;
            public long TimeSpawned;
            public long TimeDied;
            public float Mass;

            public ClientState(Socket _socket, string _name, HashSet<string> uid, int team)
            {
                Socket = _socket;
                Name = _name;
                UIDS = uid;
                TeamID = team;
                timer = 0;
            }

            public int GetLifeTime()
            {
                return Convert.ToInt32((TimeSpawned / 1000) - (TimeDied / 1000));
            }
        }
    }
}
