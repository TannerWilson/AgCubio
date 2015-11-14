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
namespace View
{
    public partial class AdCubioForm : Form
    {
        delegate void ReceiveDelegate(string hi);
        World TheWorld;

        Network_Controller.PreservedState TheState;

        // Brush used to draw
        private System.Drawing.SolidBrush myBrush;

        private int GameState = 0;

        private int count;

        int NumOfFrames = 0;
        int LastTime;

        int Mousex;
        int Mousey;

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

        void Receive(string Buffer)
        {
            lock (ReceiveLock)
            {

                // Initial receive
                if (Buffer == "Connected")
                {


                    // Send player name
                    Network.Send(TheState.TheSocket, NameTextBox.Text);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        LogInPanel.Visible = false;
                        GameState = 1;
                    }));


                }
                else // Receive data
                {
                    TheWorld.makeCube(Buffer);
                    Network.i_want_more_data(TheState);
                }
            }
        }

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
                //Close();
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
            {
                StartConnect();
            }
        }

        /// <summary>
        /// Key event in Player name text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                StartConnect();
            }
        }

        /// <summary>
        /// Called to draw background
        /// </summary>
        private void AdCubioForm_Paint(object sender, PaintEventArgs e)
        {

            if (GameState == 1)
            {
                count++;
                // sendRequest("move", Control.MousePosition.X, Control.MousePosition.Y);

                if (TheWorld.PlayerCubes != null)
                {
                    

                    // Draw all other cubes
                    foreach (Cube cube in TheWorld.GetCubeValues())
                    {
                        

                        Color color = Color.FromArgb(255, 0, 0, 1);
                        myBrush = new System.Drawing.SolidBrush(Color.Blue);
                        // Draw cube
                        e.Graphics.FillRectangle(myBrush, cube.X - (cube.getWidth() / 2), cube.Y - (cube.getWidth() / 2), cube.getWidth() + 5, cube.getWidth() + 5);
                    }
                    foreach (Cube cube in TheWorld.GetPlayerCubes())
                    {
                        if (cube.Mass != 0)
                        {
                            Color color = Color.FromArgb(255, 0, 0, 1);
                            myBrush = new System.Drawing.SolidBrush(Color.Black);
                            // Draw cube
                            float width = cube.getWidth();
                            e.Graphics.FillRectangle(myBrush, cube.X - (cube.getWidth() / 2), cube.Y - (cube.getWidth() / 2), cube.getWidth(), cube.getWidth());
                            myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
                        }
                        e.Graphics.DrawString(cube.Name, new Font("Times New Roman", 15.0f), myBrush, new PointF(cube.X, cube.Y));

                    }

                }
                // Get player's cube
                sendRequest("move", Mousex, Mousey);
                Invalidate();
                CalcFrames();

            }
        }

        void CalcFrames()
        {
            int NewTime = DateTime.Now.Second;
            NumOfFrames++;
            if (NewTime - LastTime > 1)
            {
                Debug.WriteLine("Frames: " + NumOfFrames);
                NumOfFrames = 0;
                LastTime = DateTime.Now.Second;
            }
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
            Mousex = e.X;
            Mousey = e.Y;

        }

        private void AdCubioForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                
                sendRequest("split", Mousex, Mousey);
            }
        }
    }
}
