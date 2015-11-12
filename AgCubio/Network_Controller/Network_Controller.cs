using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using Model;

namespace Network_Controller
{
    /// <summary>
    /// Object used to represent the current state of the world.
    /// Used to "know" what to do when information comes in form the server.
    /// </summary>
    public class PreservedState
    {
        /// <summary>
        /// Socket used by the state object
        /// </summary>
        public Socket TheSocket;

        /// <summary>
        /// Maximum buffer size
        /// </summary>
        public readonly int MAXBUFFERSIZE = 1024;

        /// <summary>
        /// Buffer array used in socket
        /// </summary>
        public byte[] Buffer = new byte[1024];

        /// <summary>
        /// String builder used for socked building
        /// </summary>
        public StringBuilder sb;

        /// <summary>
        /// Stores what function to call when a connection is made
        /// </summary>
        public Delegate Callback;

        /// <summary>
        /// Constructs a state object
        /// </summary>
        public PreservedState(Socket NewSocket, Delegate Receive)
        {
            TheSocket = NewSocket;
            sb = new StringBuilder();

            Callback = Receive;
        }

    }

    /// <summary>
    /// Generic class used to establish socket connections to a server
    /// </summary>
    public static class Network
    {
        // Encoding used in networking
        private static System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

        /// <summary>
        /// This function attempts to connect to the server via a provided hostname. 
        /// It saves the callback function (in a state object) for use when data arrives.
        /// </summary>
        /// <param name="callback"> A function inside the View to be called when a connection is made </param>
        /// <param name="hostname"> The name of the server to connect to </param>
        public static Socket Connect_to_Server(Delegate callback, string hostname)
        {
            Socket socket = null;
            // Attempt to establish a socket connection

            IPAddress host = IPAddress.Parse(hostname);
            IPEndPoint hostep = new IPEndPoint(host, 11000);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            PreservedState NewPreserved = new PreservedState(socket, callback);

            socket.BeginConnect(hostname, 11000, new AsyncCallback(Connected_to_Server), NewPreserved);


            return socket;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state_in_an_ar_object"></param>
        public static void Connected_to_Server(IAsyncResult state_in_an_ar_object)
        {
            try
            {
                // Grab state object from param
                PreservedState TheState = (PreservedState)state_in_an_ar_object.AsyncState;
                // End the connection
                TheState.TheSocket.EndConnect(state_in_an_ar_object);

                TheState.Callback.DynamicInvoke("Connected");
                // Start receiving state
                TheState.TheSocket.BeginReceive(TheState.Buffer, 0, TheState.MAXBUFFERSIZE, 0, new AsyncCallback(ReceiveCallback), TheState);
            }
            catch(Exception e)
            {
                
            }
        }

        /// <summary>
        /// This method should check to see how much data has arrived. If 0, 
        /// the connection has been closed (presumably by the server). On greater than zero data, 
        /// this method should call the callback function provided above
        /// </summary>
        /// <param name="state_in_an_ar_object"></param>
        public static void ReceiveCallback(IAsyncResult state_in_an_ar_object)
        {
            // Get the state back from prameter
            PreservedState TheState = (PreservedState)state_in_an_ar_object.AsyncState;
            // Get number of bytes recieved and end the receive
            int BytesRead = TheState.TheSocket.EndReceive(state_in_an_ar_object);
            // Get the buffer returned from the server
            string bufferToString = encoding.GetString(TheState.Buffer);

            TheState.Callback.DynamicInvoke(bufferToString);

        }

        /// <summary>
        /// Helper function that the client View code will call whenever it wants more data.
        /// Note: the client will probably want more data every time it gets data
        /// </summary>
        public static void i_want_more_data(PreservedState state)
        {
            state.TheSocket.BeginReceive(state.Buffer, 0, state.MAXBUFFERSIZE, 0, new AsyncCallback(ReceiveCallback), state);
        }

        /// <summary>
        /// This function will allow a program to send data over a socket. 
        /// This function converts the data into bytes and then sends them using socket.BeginSend.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        public static void Send(Socket socket, String data)
        {
            byte[] byteData = encoding.GetBytes(data);

            // Begin sending the data to the remote device.
            socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallBack), socket);
        }

        /// <summary>
        /// This function "assists" the Send function. If all the data has been sent, 
        /// then life is good and nothing needs to be done. If there is more data to send, 
        /// the SendCallBack needs to arrange to send this data.
        /// </summary>
        public static void SendCallBack(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
