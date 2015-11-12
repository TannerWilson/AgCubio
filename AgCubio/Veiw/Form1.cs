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
        
        /// <summary>
        /// Creates new AgCubio game and form
        /// </summary>
        public AdCubioForm()
        {
            // Make new world
            TheWorld = new World(1000, 1000, new HashSet<Cube>());
            InitializeComponent();
            // Delagate called when connection is made
            ReceiveDelegate ReceiveCallBack = new ReceiveDelegate(Receive);
            try
            {
                Socket NewSocket = Network.Connect_to_Server(ReceiveCallBack, "127.0.0.1");
                TheState = new Network_Controller.PreservedState(NewSocket, ReceiveCallBack);
            }
            catch(Exception e)
            {
                MessageBox.Show("Unable to connect to server.\n" + e.Message);
                Close();
            }


        }

        void Receive(string Buffer)
        {
            
            if (Buffer == "Connected")
            {
                Network.Send(TheState.TheSocket, "Hiphop\n");
            }
            else
            {
                TheWorld.makeCube(Buffer);
                Network.i_want_more_data(TheState);
            }
        }

    }
}
