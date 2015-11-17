using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using Network_Controller;
using Model;
using Newtonsoft.Json;
using System.Threading;
namespace View
{
    public partial class AdCubioForm : Form
    {
        delegate void ReceiveDelegate(PreservedState State);
        // Used to represent the current game
        private World TheWorld;
        PreservedState TheState;

        // Brush used to draw
        private System.Drawing.SolidBrush myBrush;
        // Shows when the game is being played
        private int GameState = 0;

        private int count;
        // Used to calculate FPS
        private int NumOfFrames = 0;
        private int LastTime;
        // Players current mass
        private int PlayerMass;
        // Mouse location
        private int Mousex;
        private int Mousey;
        // Thread lock
        private Object ReceiveLock = new Object();

        /// <summary>
        /// Creates new AgCubio game and form
        /// </summary>
        public AdCubioForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            LastTime = DateTime.Now.Second;
        }

        /// <summary>
        /// Called when information is recieved from the server.
        /// </summary>
        /// <param name="State"></param>
        void Receive(PreservedState State)
        {
            lock (ReceiveLock)
            {
                // Initial receive
                if (GameState == 0)
                {
                    // Send player name
                    Network.Send(State.TheSocket, NameTextBox.Text);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        LogInPanel.Visible = false;
                        GameState = 1;
                    }));
                }
                else // Receive data
                {
                    lock(State.sb)
                    {
                        lock (TheWorld)
                        {
                            // Split String
                            string[] SplitString = State.sb.ToString().Split('\n');

                            // Go through every split string item.
                            foreach (string Item in SplitString)
                            {
                                // See if the item is a complete string
                                if (Item.StartsWith("{") && Item.EndsWith("}"))
                                {
                                    try
                                    {
                                        //(uncomment for debuging)
                                        //Cube adding = JsonConvert.DeserializeObject<Cube>(Item);
                                        TheWorld.makeCube(Item);
                                    }
                                    catch (Exception e) { }
                                }
                                else // The item is not complete.
                                {
                                    // Clear the the string builder.
                                    State.sb.Clear();
                                    // Append the incomplete item to the string builder. 
                                    State.sb.Append(Item);
                                }
                            }
                            // Get more data.
                            Network.i_want_more_data(State);
                        }
                    }                   
                }
            }
        }

        /// <summary>
        /// Creates the game world and establishes a connection with the server.
        /// </summary>
        void StartConnect()
        {
            // Make new world
            TheWorld = new World(1000, 1000);

            // Delagate called when connection is made
            ReceiveDelegate ReceiveCallBack = new ReceiveDelegate(Receive);
            try
            {
                Socket NewSocket = Network.Connect_to_Server(ReceiveCallBack, ServerTextBox.Text);
                TheState = new Network_Controller.PreservedState(NewSocket, ReceiveCallBack);
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to connect to server.\n" + e.Message);
            }
        }

        /// <summary>
        /// Key event in server text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                StartConnect();
        }

        /// <summary>
        /// Key event in Player name text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                StartConnect();
        }

        /// <summary>
        /// Called to draw background
        /// </summary>
        private void AdCubioForm_Paint(object sender, PaintEventArgs e)
        {
            if (GameState == 1)
            {
                count++;
                if (TheWorld.PlayerCubes != null)
                {                    
                    // Draw all other cubes
                    foreach (Cube cube in TheWorld.GetCubeValues())
                    {    
                        // Set brush color                   
                        myBrush = new SolidBrush(Color.Blue);
                        // Draw cube
                        e.Graphics.FillRectangle(myBrush, cube.X - (cube.getWidth() / 2), cube.Y - (cube.getWidth() / 2), cube.getWidth() + 5, cube.getWidth() + 5);
                    }
                    // Draw all player cubes
                    foreach (Cube cube in TheWorld.GetPlayerCubes())
                    {
                        if (cube.Mass != 0)
                        {
                            // Set new player mass
                            PlayerMass = (int) cube.Mass;
                            MassLabel.Text = "Mass: " + PlayerMass;
                            MassLabel.Refresh();
                            // Set brush to cube color
                            myBrush = new System.Drawing.SolidBrush(Color.Black);
                            // Draw cube
                            float width = cube.getWidth();
                            e.Graphics.FillRectangle(myBrush, cube.X - (cube.getWidth() / 2), cube.Y - (cube.getWidth() / 2), cube.getWidth(), cube.getWidth());
                            myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
                        }
                        e.Graphics.DrawString(cube.Name, new Font("Times New Roman", 15.0f), myBrush, new PointF(cube.X, cube.Y));
                    }
                }
                // Send move request
                sendRequest("move", Mousex, Mousey);
                // Calculate FPS then paint
                CalcFrames();
                Invalidate();                
            }
        }

        /// <summary>
        /// Calculates the current frames per second
        /// </summary>
        void CalcFrames()
        {
            int NewTime = DateTime.Now.Second;
            NumOfFrames++;
            if (NewTime - LastTime > 1)
            {
                FramesLabel.Text = "FPS: " + NumOfFrames;
                FramesLabel.Refresh();
                NumOfFrames = 0;
                LastTime = DateTime.Now.Second;
            }
        }

        /// <summary>
        /// Calculates the offest used to scale and "zoom" the world view
        /// </summary>
        public int CalcOffset()
        {
            // TODO: Finish this shizzzzz
            int offSet = PlayerMass * 10;

            return offSet;
        }

        /// <summary>
        /// Sends request to the server
        /// </summary>
        /// <param name="request"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void sendRequest(string request, int x, int y)
        {
            if (GameState == 1)
            {
                string message = "(" + request + ", " + x + "," + y + ")\n";
                Network.Send(TheState.TheSocket, message);
            }
        }

        /// <summary>
        /// Mouse move event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AdCubioForm_MouseMove(object sender, MouseEventArgs e)
        {
            // Record the curent mouse location
            Mousex = e.X;
            Mousey = e.Y;
        }

        /// <summary>
        /// Key pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AdCubioForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Send split request when space is pressed
            if (e.KeyCode == Keys.Space)
            {               
                sendRequest("split", Mousex, Mousey);
            }
        }
    }
}
