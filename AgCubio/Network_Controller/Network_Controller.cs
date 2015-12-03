using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace Network_Controller
{

    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();

        public string Name;

        /// <summary>
        /// Action used on the server side of connections
        /// </summary>
        public Action<StateObject> ServerCallback;

    }

    public static class Network
    {
        public static void ConnectToServer(Action<StateObject> Callback, string Hostname)
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Hostname);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.
            Socket client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            StateObject NewStateObject = new StateObject();
            NewStateObject.ServerCallback = Callback;
            NewStateObject.workSocket = client;

            client.BeginConnect(remoteEP, ConnectCallback, NewStateObject);


        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                StateObject State = (StateObject)ar.AsyncState;
                Socket client = State.workSocket;

                // Complete the connection.
                client.EndConnect(ar);

                // We connected now notify!
                State.ServerCallback(State);

                


            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public static void Send(Socket client, String data)
        {
            lock (client)
            {
                // Convert the string data to byte data using ASCII encoding.
                byte[] byteData = Encoding.UTF8.GetBytes(data);

                // Begin sending the data to the remote device.
                client.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), client);
            }

        }

        private static void SendCallback(IAsyncResult ar)
        {

            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                lock (client)
                {
                    // Complete sending the data to the remote device.
                    int bytesSent = client.EndSend(ar);
                    
                }


            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                lock (state.workSocket)
                {
                    // Begin receiving the data from the remote device.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                lock (state.sb)
                {

                    Socket client = state.workSocket;

                    // Read data from the remote device.
                    int bytesRead = client.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        string StringData = Encoding.UTF8.GetString(state.buffer, 0, bytesRead);
                        state.sb.Append(StringData);
                        state.ServerCallback(state);
                    }
                    else
                    {

                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void GetData(StateObject state)
        {
            Socket client = state.workSocket;
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }

        /// <summary>
        /// Asks the OS to listen for a connection and saves the callback function with that request. 
        /// Upon a connection request coming in the OS should invoke the Accept_a_New_Client method 
        /// </summary>
        /// <param name="callback"></param>
        public static void Server_Awaiting_Client_Loop(Action<StateObject> CallBack)
        {
            // Connect to server
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint clientep = new IPEndPoint(IPAddress.Any, 11000);

            Socket ServerConnect;

            // Create socket to connect
            ServerConnect = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Console.WriteLine("Server has started!");

                // Create "handshake"
                ServerConnect.Bind(clientep);
                ServerConnect.Listen(10);


                // Creat new state object, and start callback           
                StateObject State = new StateObject();
                State.workSocket = ServerConnect;
                State.ServerCallback = CallBack;

                // Start connection
                ServerConnect.BeginAccept(Accept_a_New_Client, State);
            }
            catch (Exception e)
            {
                Console.WriteLine("Server_Awaiting_Client_Loop ERROR: .\n" + e.Message);
            }

        }

        /// <summary>
        /// Creates a new socket, Calls the callback provided by the above method
        /// and awaits a new connection request.
        /// </summary>
        /// <param name="ar"></param>
        public static void Accept_a_New_Client(IAsyncResult ar)
        {
            try
            {
                // Pull state from result
                StateObject listener = (StateObject)ar.AsyncState;
                // New socket representing the client connection
                Socket ClientSocket = listener.workSocket.EndAccept(ar);

                StateObject ClientState = new StateObject();
                ClientState.workSocket = ClientSocket;
                ClientState.ServerCallback = listener.ServerCallback;

                ClientState.ServerCallback(ClientState);


                // Listener thread
                listener.workSocket.BeginAccept(Accept_a_New_Client, listener);

            }
            catch (Exception e)
            {
                Console.WriteLine("Accept Failed! -> \n" + e.Message);
            }
        }

    }
}
