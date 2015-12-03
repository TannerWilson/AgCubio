using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Network_Controller;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Model;
namespace AgCubio
{
    public partial class AgCubioForm : Form
    {
        World GameWorld;
        Socket WorkSocket;

        int Mousex, Mousey;
        int GameState;


        // Used to calculate FPS
        private int NumOfFrames = 0;
        private int LastTime;

        // Players current mass and width
        private float PlayerMass = 0;
        private float PlayerWidth;

        private string PlayerName;

        bool FirstPlayer;

        public AgCubioForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            GameWorld = new World(1000, 1000);
            GameState = 0;
       
            
        }

        private void StartConnect(string HostName)
        {
            Network.ConnectToServer(ConnectedToServerCallback, HostName);
            PlayerName = NameTextBox.Text;
        }

        private void ConnectedToServerCallback(StateObject ServerState)
        {
            // We have connect to server!
            this.Invoke(new MethodInvoker(delegate
            {
                ButtonPanel.Visible = false;
                NameTextBox.Enabled = false;
                textBox1.Enabled = false;
            }));

            

            Socket ServerSocket = ServerState.workSocket;

            WorkSocket = ServerSocket;

            Network.Send(ServerSocket, PlayerName +"\n");

            ServerState.ServerCallback = ReceiveServerData;

            Network.GetData(ServerState);
            GameState = 1;

        }

    

        private void ReceiveServerData(StateObject ServerState)
        {
            lock (ServerState.sb)
            {
               
                
                string[] SplitData = ServerState.sb.ToString().Split('\n');
                ServerState.sb.Clear();
                foreach (string StringItem in SplitData)
                {
                    //Debug.WriteLine(StringItem);
                    if (StringItem.StartsWith("{") && StringItem.EndsWith("}"))
                    {
                        lock (GameWorld)
                        {
                            GameWorld.AddCube(StringItem);
                        }
                    }
                    else
                    {
                        ServerState.sb.Append(StringItem);
                    }
                }
            }
            Network.GetData(ServerState);
        }

        private void AgCubioForm_Paint(object sender, PaintEventArgs e)
        {
            lock (GameWorld)
            {
                if (GameState == 1)
                {
                    foreach (Cube cube in GameWorld.DictionaryOfCubes.Values)
                    {
                        SolidBrush myBrush = new SolidBrush(Color.FromArgb(cube.argb_color));
                        // Draw cube
                        e.Graphics.FillRectangle(myBrush, cube.loc_x - (cube.getWidth() / 2), cube.loc_y - (cube.getWidth() / 2), cube.getWidth(), cube.getWidth());
                        SolidBrush FontBrush = new SolidBrush(Color.Yellow);
                        e.Graphics.DrawString(cube.Name, new Font("Times New Roman", 15.0f), FontBrush, new PointF(cube.loc_x - 20, cube.loc_y));

                    }
                    foreach (Cube cube in GameWorld.FoodCubes.Values)
                    {
                        SolidBrush myBrush = new SolidBrush(Color.FromArgb(cube.argb_color));
                        // Draw cube
                        e.Graphics.FillRectangle(myBrush, cube.loc_x - (cube.getWidth() / 2), cube.loc_y - (cube.getWidth() / 2), cube.getWidth() + 1, cube.getWidth() + 1);
                    }
                    //Send Request
                    sendRequest("move",Mousex,Mousey);
                    Invalidate();
                    CalcFrames();
                }
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
                FPSAmountLabel.Text = NumOfFrames.ToString();
                FPSAmountLabel.Refresh();
                NumOfFrames = 0;
                LastTime = DateTime.Now.Second;
            }
        }

        private void AgCubioForm_MouseMove(object sender, MouseEventArgs e)
        {
            // Record the curent mouse location
            Mousex = e.X;
            Mousey = e.Y;
        }

        private void AgCubioForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                sendRequest("split", Mousex, Mousey);
            }
        }

        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                StartConnect(textBox1.Text);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                StartConnect(textBox1.Text);
            }
        }

        private void ButtonPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            
        }

        private void sendRequest(string request, int x, int y)
        {
            string message = "(" + request + ", " + x + "," + y + ")\n";
            Network.Send(WorkSocket, message);
        }
    }
}
