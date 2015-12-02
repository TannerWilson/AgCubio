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
    /// <summary>
    /// Form used to paint the AgCubio game.
    /// </summary>
    public partial class AgCubioForm : Form
    {
        delegate void ReceiveDelegate(PreservedState State);
        // Used to represent the current game
        private World TheWorld;
        PreservedState TheState;

        // Brush used to draw
        private System.Drawing.SolidBrush myBrush;
        // Shows when the game is being played
        private int GameState = 0;

        // Used to calculate FPS
        private int NumOfFrames = 0;
        private int LastTime;

        // Players current mass and width
        private float PlayerMass = 0;
        private float PlayerWidth;
        // Mouse location
        private int Mousex;
        private int Mousey;

        /// <summary>
        /// Creates new AgCubio game and form
        /// </summary>
        public AgCubioForm()
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

            // Initial receive
            if (GameState == 0)
            {
                if (State.sb.ToString() != "Could not connect!")
                {
                    // Send player name
                    Network.Send(State.TheSocket, NameTextBox.Text+"\n");

                    this.Invoke(new MethodInvoker(delegate
                    {
                        // Update form views
                        LogInPanel.Visible = false;
                    }));

                    GameState = 1;

                }
                else
                {
                    MessageBox.Show("Could not connect to server!");
                    State.sb.Clear();
                }

            }
            else // Receive data
            {
                string[] SplitString = null;

                lock (TheState.sb)
                {
                    // Split String
                    SplitString = State.sb.ToString().Split('\n');

                    State.sb.Clear();
                }

                // Go through every split string item.
                foreach (string Item in SplitString)
                {

                    // See if the item is a complete string
                    if (Item.StartsWith("{") && Item.EndsWith("}"))
                    {
                        lock (TheWorld)
                        {
                            TheWorld.makeCube(Item);
                        }
                    }
                    else // The item is not complete.
                    {
                        if (!Item.StartsWith("\0\0\0"))
                        {
                            // Append the incomplete item to the string builder. 
                            State.sb.Append(Item);
                        }
                    }
                }
                // Get more data.
                Network.i_want_more_data(State);
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

            Socket NewSocket = Network.Connect_to_Server(ReceiveCallBack, ServerTextBox.Text);
            TheState = new Network_Controller.PreservedState(NewSocket, ReceiveCallBack);

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
                List<Cube> PlayerList = null;
                List<Cube> CubeList = null;

                lock (TheWorld)
                {
                    PlayerList = TheWorld.PlayerCubes.Values.ToList<Cube>();
                    CubeList = TheWorld.DictionaryOfCubes.Values.ToList<Cube>();
                }

                int NumOfFood = 0;

                //// Draw all other cubes
                foreach (Cube cube in CubeList)
                {
                    if (cube.FoodStatus == true)
                    {
                        NumOfFood += 1;
                    }
                    // Set brush color                   
                    myBrush = new SolidBrush(Color.FromArgb(cube.argb_color));
                    // Draw cube
                    e.Graphics.FillRectangle(myBrush, cube.X - (cube.getWidth() / 2), cube.Y - (cube.getWidth() / 2), cube.getWidth()+3, cube.getWidth()+3);

                    // Draw player name
                    myBrush = new SolidBrush(Color.Black);
                    e.Graphics.DrawString(cube.Name, new Font("Times New Roman", 15.0f), myBrush, new PointF(cube.X - 20, cube.Y));
                }
                // Update food label
                FoodLabel.Text = NumOfFood.ToString();
                FoodLabel.Refresh();

                // Used to display current player mass
                int mass = 0;

                // Draw all player cubes
                foreach (Cube cube in PlayerList)
                {
                    // Update width and masses
                    mass += (int)cube.Mass;
                    PlayerMass += cube.Mass;
                    PlayerWidth = cube.getWidth();
                    if (cube.Mass != 0)
                    {
                        // Set brush to cube color
                        myBrush = new SolidBrush(Color.FromArgb(cube.argb_color));
                        // Draw cube
                        float width = cube.getWidth();
                        e.Graphics.FillRectangle(myBrush, cube.X - (cube.getWidth() / 2), cube.Y - (cube.getWidth() / 2), cube.getWidth(), cube.getWidth());

                        // Draw player name
                        myBrush = new SolidBrush(Color.Black);
                        e.Graphics.DrawString(cube.Name, new Font("Times New Roman", 15.0f), myBrush, new PointF(cube.X, cube.Y - 5));                       
                    }
                }

                // Player has died
                if (mass == 0 && GameState == 1 && PlayerMass != 0)
                {
                    MessageBox.Show("You died!\n Your final mass was: " + PlayerMass);
                    Close();
                }

                // Update width label
                WidthLabel.Text = "Width: " + PlayerWidth;
                WidthLabel.Refresh();

                // Update mass label
                MassLabel.Text = "Mass: " + mass;
                MassLabel.Refresh();

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
        public float CalcScale(float width)
        {

            double scale = this.Width / (width * 40);

            return (float)scale;
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

        private void AgCubioForm_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
        }
    }
}
