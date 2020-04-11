using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProtocolTranslator
{
    public partial class ProtocolTranslator : Form
    {
        ServerController server = new ServerController();
        public ProtocolTranslator()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Program.eventhandler += ShowLog;
            Program.log("Info", "I do");
        }
        private void ShowLog(object sender, LogEventArgs e)
        {
            LogForm.Text += "\n" + string.Format("[{0}] {1}", e.level, e.message);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (server.StartWebsocketServer()) {
                button1.Enabled = false;
            };

        }

    }
}
