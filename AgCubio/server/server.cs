using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network_Controller;
using View;

namespace server
{
    class Server
    {
        static private World GameWorld;

        static HashSet<PreservedState> States;

        static private int TickRate;


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
            GameWorld = new World(1000, 1000);
            Network.Server_Awaiting_Client_Loop(PlayerConnect);
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
        static public void ClientData(string request)
        {

        }

        static void PlayerConnect(PreservedState State)
        {
            State.ServerCallback = GetPlayerName;
            Network.i_want_more_data(State);
        }

        static void GetPlayerName(PreservedState State)
        {
            string[] NewLineStrings = null;
            string PlayerName = null;
            lock (State.sb)
            {
                NewLineStrings = State.sb.ToString().Split('\n');
                State.sb.Clear();
            }

            if (NewLineStrings != null)
            {
                PlayerName = NewLineStrings[0]; 
            }

            lock(GameWorld)
            {
                Console.WriteLine("Player "+PlayerName + " has entered the game!");
                //Make Player Cube
            }
           

        }

        /// <summary>
        /// Updates all aspects of the world.
        /// </summary>
        static public void Update()
        {

        }
    }
}
