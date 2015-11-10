using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Network_Controller
{
    /// <summary>
    /// Object used to represent the current state of the world.
    /// Used to "know" what to do when information comes in form the server.
    /// </summary>
    public class PreservedState
    {
        public Socket TheSocket;
        public readonly int MAXBUFFERSIZE = 1024;
        public byte[] Buffer;

        public PreservedState(Socket NewSocket)
        {
            TheSocket = NewSocket;
        }

        
        
    }

    public static class Network
    {

        /// <summary>
        /// This function attempts to connect to the server via a provided hostname. 
        /// It saves the callback function (in a state object) for use when data arrives.
        /// </summary>
        /// <param name="callback"> A function inside the View to be called when a connection is made </param>
        /// <param name="hostname"> The name of the server to connect to </param>
        public static Socket Connect_to_Server(Delegate callback, string hostname)
        {
            Socket socket = null;
            try
            {
                IPAddress host = IPAddress.Parse("127.0.0.1");
                IPEndPoint hostep = new IPEndPoint(host, 11000);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                PreservedState NewPreserved = new PreservedState(socket);
                socket.BeginConnect(hostname, 11000, Connected_to_Server, NewPreserved);
            }
            catch(Exception e)
            {

            }
            

            return socket;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state_in_an_ar_object"></param>
        public static void Connected_to_Server(IAsyncResult state_in_an_ar_object)
        {
            PreservedState TheState = (PreservedState)state_in_an_ar_object.AsyncState;
            Socket socket = TheState.TheSocket;
         

            Socket NewSocket =  socket.EndAccept(state_in_an_ar_object);
            SocketError Error;
            
            socket.BeginReceive(TheState.Buffer, 0, TheState.MAXBUFFERSIZE, 0, out Error, ReceiveCallback, TheState);
        }

        /// <summary>
        /// This method should check to see how much data has arrived. If 0, 
        /// the connection has been closed (presumably by the server). On greater than zero data, 
        /// this method should call the callback function provided above
        /// </summary>
        /// <param name="state_in_an_ar_object"></param>
        public static void ReceiveCallback(IAsyncResult state_in_an_ar_object)
        {

        }

        /// <summary>
        /// Helper function that the client View code will call whenever it wants more data.
        /// Note: the client will probably want more data every time it gets data
        /// </summary>
        public static void i_want_more_data(PreservedState state)
        {

        }

        /// <summary>
        /// This function will allow a program to send data over a socket. 
        /// This function converts the data into bytes and then sends them using socket.BeginSend.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        public static void Send(Socket socket, String data)
        {

        }

        /// <summary>
        /// This function "assists" the Send function. If all the data has been sent, 
        /// then life is good and nothing needs to be done. If there is more data to send, 
        /// the SendCallBack needs to arrange to send this data.
        /// </summary>
        public static void SendCallBack()
        {

        }
    }
}
