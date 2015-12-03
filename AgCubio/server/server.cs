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

namespace Server
{
    class Server
    {

        static private World GameWorld;

        static HashSet<StateObject> States;

        static private int TickRate;

        static Timer timer;

        // Clients connected to the game
        static Dictionary<string, ClientState> Clients;

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
            }

            lock (State)
            {
                State.Name = PlayerName;
                State.ServerCallback = ClientData;
            }


            // Send palyer
            Network.Send(State.workSocket, PlayerJsonString + "\n");
            // Send food data
            //SendInitFoodData(State);



            Network.GetData(State);
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
                                    if (Int32.TryParse(RequestSplitString[1].ToString(), out x) && Int32.TryParse(RequestSplitString[1].ToString(), out y))
                                    {

                                        ClientState PlayerState = Clients[State.Name];


                                        foreach (string UID in PlayerState.UID)
                                        {
                                            GameWorld.ActionCommand(RequestSplitString[0], x, y, UID);
                                            Console.WriteLine(x +", " + y);
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

        static void PlayerConnect(StateObject State)
        {
            State.ServerCallback = GetPlayerName;
            Network.GetData(State);
        }

        /// <summary>
        /// Updates all aspects of the world.
        /// </summary>
        static public void Update(object source, ElapsedEventArgs e)
        {
            lock (GameWorld)
            {

                List<string> UpdatedCubes = new List<string>();
                // Creat new food cube and send to each client
                string FoodCube = GameWorld.GenerateNewFood();
                foreach (ClientState state in Clients.Values)
                {
                    if (FoodCube != null)
                        Network.Send(state.Socket, FoodCube);
                }


                foreach (Cube CubeItem in GameWorld.DictionaryOfCubes.Values)
                {
                    string UpdatedJsonCube = CubeItem.Update();

                    if (UpdatedJsonCube != null)
                    {
                        UpdatedCubes.Add(UpdatedJsonCube);
                    }
                }

                foreach (ClientState state in Clients.Values)
                {
                    foreach (string UpdatedCubeItem in UpdatedCubes)
                    {
                        ///Console.WriteLine("Sending DATA");
                        Network.Send(state.Socket, UpdatedCubeItem);
                    }
                }
            }


        }



        protected class ClientState
        {
            public Socket Socket;
            public string Name;
            public HashSet<string> UID;
            public int TeamID;

            public ClientState(Socket _socket, string _name, HashSet<string> uid, int team)
            {
                Socket = _socket;
                Name = _name;
                UID = uid;
                TeamID = team;
            }
        }
    }
}
