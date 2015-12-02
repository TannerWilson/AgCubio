using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network_Controller;
using View;
using Newtonsoft.Json;
using System.Timers;
using System.Net.Sockets;

namespace server
{
    class Server
    {
        static private World GameWorld;

        static HashSet<PreservedState> States;

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
        /// Sets up a callback to receive a players name and then request more data from the connection
        /// </summary>
        static public void ClientConnection()
        {

        }

        /// <summary>
        /// Creates the new player cube (and updates the world about it) 
        /// and stores away all the necessary data for the connection to be used for further communication. 
        /// Also sets up the callback for  handling move/split requests and request new data from 
        /// the socket. Finally it should send the current state of the world to the player.
        /// </summary>
        /// <returns></returns>
        static public Cube ReceivePalyer(string name)
        {
            Cube Player = null;
            return Player;
        }

        /// <summary>
        /// Receives data from clients. Should be either (move,x,y) or (split,x,y). 
        /// </summary>
        /// <param name="request"></param>
        static public void ClientData(PreservedState State)
        {
            string[] SplitStrings = null;
            string ActionRequest = null;
            // Process data recieved
            lock (State.sb)
            {
                SplitStrings = State.sb.ToString().Split('\n');
                State.sb.Clear();
            }
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
                    // Ensure the request is not empty
                    if (RequestSplitString[0] != "")
                    {
                        // Ensure request is formatted correctly
                        int x, y;
                        if (Int32.TryParse(RequestSplitString[1].ToString(), out x) && Int32.TryParse(RequestSplitString[1].ToString(), out y))
                        {
                            ClientState PlayerState = Clients[State.Name];
                            foreach (string UID in PlayerState.UID)
                            {
                                GameWorld.ActionCommand(RequestSplitString[0], x, y, UID);
                            }

                        }
                        else
                        {
                            Console.WriteLine("Inalid request recieved.");
                        }
                    }
                }
            }
            // Ask for more data after processing all previous requests
            Network.ServerWantsMoreData(State);
        }

        static void PlayerConnect(PreservedState State)
        {
            State.ServerCallback = GetPlayerName;
            Network.ServerWantsMoreData(State);
        }

        /// <summary>
        /// Recieve the player name, create player cube and send player string.
        /// </summary>
        /// <param name="State"></param>
        static void GetPlayerName(PreservedState State)
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
                NewPlayerHash.Add(PlayerCube.GetUID());

                ClientState NewClient = new ClientState(State.TheSocket, PlayerName, NewPlayerHash, PlayerCube.team_id);
                Clients.Add(PlayerName, NewClient);
            }

            lock (State)
            {
                State.Name = PlayerName;
            }


            // Send palyer
            Network.Send(State.TheSocket, PlayerJsonString + "\n");
            // Send food data
            SendInitFoodData(State);

            State.ServerCallback = ClientData;

            Network.ServerWantsMoreData(State);



        }

        /// <summary>
        /// Send all food cubes as JSON strings
        /// </summary>
        /// <param name="State"></param>
        static void SendInitFoodData(PreservedState State)
        {
            lock (GameWorld)
            {
                foreach (Cube FoodItem in GameWorld.FoodCubes.Values)
                {
                    Network.Send(State.TheSocket, JsonConvert.SerializeObject(FoodItem) + "\n");
                }
            }
        }

        /// <summary>
        /// Updates all aspects of the world.
        /// </summary>
        static public void Update(object source, ElapsedEventArgs e)
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
