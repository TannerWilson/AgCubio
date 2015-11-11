using System;
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
namespace View
{
    public partial class Form1 : Form
    {
        public Form1()
        {
       
            InitializeComponent();
            Delegate FuckIt = null;
            Socket TheSocket = Network.Connect_to_Server(FuckIt, "127.0.0.1");

            Network.Send(TheSocket, "Hiphop\n");

            
        }
    }
}
