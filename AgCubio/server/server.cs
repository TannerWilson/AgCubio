using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class Server
    {
        static void Main(string[] args)
        {

        }

        /// <summary>
        /// Populates the initial world (with food), 
        /// set up the heartbeat of the program, and await network client connections. 
        /// </summary>
        public void start()
        {

        }

        /// <summary>
        /// Sets up a callback to receive a players name and then request more data from the connection
        /// </summary>
        public void ClientConnection()
        {

        }

        /// <summary>
        /// Creates the new player cube (and updates the world about it) 
        /// and stores away all the necessary data for the connection to be used for further communication. 
        /// Also sets up the callback for  handling move/split requests and request new data from 
        /// the socket. Finally it should send the current state of the world to the player.
        /// </summary>
        /// <returns></returns>
        public Cube ReceivePalyer(string name)
        {
            Cube Player = null;
            return Player;
        }

        /// <summary>
        /// Receives data from clients. Should be either (move,x,y) or (split,x,y). 
        /// </summary>
        /// <param name="request"></param>
        public void ClientData(string request)
        {

        }

        /// <summary>
        /// Updates all aspects of the world.
        /// </summary>
        public void Update()
        {

        }
    }
}
