using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProtocolTranslator
{
    public partial class ProtocolTranslator : Form
    {
        string a;
        ServerController server = new ServerController();
        public ProtocolTranslator()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            a = LogForm.Text;
            LogForm.Text = "";
            Program.eventhandler += ShowLog;
            Program.log("Info", "I do");
        }
        private void ShowLog(object sender, LogEventArgs e)
        {
            if (LogForm.InvokeRequired)
            {
                Action<LogEventArgs> action = (ev) =>
                {
                    LogForm.AppendText(string.Format("[{0}] {1}", ev.level, ev.message));
                    LogForm.AppendText(a);
                };
                this.LogForm.Invoke(action, e);
            }
            else
            {
                LogForm.Text += string.Format("[{0}] {1}", e.level, e.message) + a;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Uri url = new Uri(textBox1.Text);
                if(url.Scheme != "ws")
                {
                    MessageBox.Show("远程地址不是一个有效的ws地址哦！");
                    return;
                }
                ServerController.url = textBox1.Text;
            }
            catch (Exception)
            {
                MessageBox.Show("远程地址不是一个有效的ws地址哦！");
                throw;
            }
            try
            {
                IPAddress address = IPAddress.Parse(textBox2.Text);
                ServerController.localhost = address;
            }
            catch (Exception)
            {
                MessageBox.Show("监听地址不是一个有效的IP地址哦！");
                throw;
            }
            ServerController.localport = (int)numericUpDown1.Value;
            if (server.StartWebsocketServer())
            {
                button1.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                numericUpDown1.Enabled = false;
            };

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 火绒的修改Hosts文件就是打开记事本，哈哈哈哈哈哈
            Process p = new Process();
            p.StartInfo.FileName = "notepad.exe";
            p.StartInfo.Verb = "runas";
            p.StartInfo.Arguments = "C:\\WINDOWS\\system32\\drivers\\etc\\hosts";
            p.Start();
        }

        private void LogForm_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
