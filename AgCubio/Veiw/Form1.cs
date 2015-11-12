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

        /// <summary>
        /// Creates new AgCubio game and form
        /// </summary>
        public AdCubioForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            
        }

        void Receive(string Buffer)
        {
            // Initial receive
            if (Buffer == "Connected")
            {
                // Send player name
                Network.Send(TheState.TheSocket, NameTextBox.Text);
                
            }
            else // Receive data
            {              
                TheWorld.makeCube(Buffer);
                Network.i_want_more_data(TheState);
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
                Close();
            }
            LogInPanel.Visible = false;
            GameState = 1;
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

                if (TheWorld.PlayerCubes != null && TheWorld.DictionaryOfCubes.Count !=0)
                {
                    // Get player's cube
                    

                    int width = (int)TheWorld.PlayerCubes.getWidth();

                    Color color = Color.FromArgb(0, 0, 0);
                    myBrush = new System.Drawing.SolidBrush(color);
                    // Draw player cube
                    e.Graphics.FillRectangle(myBrush, new Rectangle((int)TheWorld.PlayerCubes.X/4, (int)TheWorld.PlayerCubes.Y/4, Width/7, Width/7));
                }
                Invalidate();

                
                // Draw all other cubes
                foreach (KeyValuePair<string, Cube> item in TheWorld.DictionaryOfCubes)
                {
                    Cube cube = item.Value;
                    // Get and set color
                    Color color = Color.FromArgb(cube.Color);
                    myBrush = new System.Drawing.SolidBrush(color);
                    // Draw cube
                    e.Graphics.FillRectangle(myBrush, new Rectangle((int)cube.X, (int)cube.Y, (int)cube.getWidth(), (int)cube.getWidth()));

                }
            }
            
        }
    }
}
