using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// Multiplayer form for the guest player.
    /// </summary>
    public partial class JoinForm : Form
    {
        private int tries;
        public TcpClient Client { get; set; }

        public JoinForm()
        {
            InitializeComponent();

            this.Client = new TcpClient();
            this.label1.Text = Properties.Resources.Port;
            this.label2.Text = Properties.Resources.Address;
            this.button1.Text = Properties.Resources.Accept;
            this.button2.Text = Properties.Resources.Cancel;
            this.Text = Properties.Resources.JoinTitle;
        }

        /// <summary>
        /// OK button event handler. Enables the timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            System.Net.IPAddress tmp;
            if (System.Net.IPAddress.TryParse(this.ipText.Text, out tmp))
            {
                this.tries = 0;
                this.timer1.Enabled = true;
                this.button1.Enabled = false;
                this.ipText.Enabled = false;
                this.portUpDown.Enabled = false;
            }
            else
                MessageBox.Show(Properties.Resources.InvalidIP);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        /// <summary>
        /// Timer's tick event handler. Tries to connect to the given host 30 times before giving up.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.tries < 30)
            {
                this.progressBar1.Value = (int)((this.tries + 1) * (this.progressBar1.Maximum - this.progressBar1.Minimum) / 30);
            }
            else
            {
                this.timer1.Enabled = false;
                MessageBox.Show(Properties.Resources.ServerError + " " + ipText.Text + ":" + portUpDown.Value);
                this.button1.Enabled = true;
                this.ipText.Enabled = true;
                this.portUpDown.Enabled = true;
                this.progressBar1.Value = 0;
            }
            try
            {
                this.Client.Connect(System.Net.IPAddress.Parse(ipText.Text), (int)portUpDown.Value);
                if (this.Client.Connected)
                {
                    this.timer1.Enabled = false;
                    this.button1.Enabled = true;
                    this.ipText.Enabled = true;
                    this.portUpDown.Enabled = true;
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                this.tries++;
            }
            
        }
        private void JoinForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Dispose();
        }
    }
}
