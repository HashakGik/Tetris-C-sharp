using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// Multiplayer form for the host player.
    /// </summary>
    public partial class HostForm : Form
    {
        private TcpListener host;
        public TcpClient Client { get; set; }
        public uint Level { get; set; }
        public uint BHeight { get; set; }
        public bool BMode { get; set; }
        public HostForm()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;

            this.label1.Text = Properties.Resources.Port;
            this.label2.Text = Properties.Resources.LevelString;
            this.label3.Text = Properties.Resources.ModeString;
            this.label4.Text = Properties.Resources.HeightString;
            this.button1.Text = Properties.Resources.Accept;
            this.button2.Text = Properties.Resources.Cancel;
            this.Text = Properties.Resources.HostTitle;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        /// <summary>
        /// OK button event handler. Starts listening on the specified TCP port for incoming connections.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                this.host = new TcpListener(System.Net.IPAddress.Any, (int)portUpDown.Value);
                this.host.Start();

                this.tries = 0;
                this.timer1.Enabled = true;
                this.button1.Enabled = false;
                this.portUpDown.Enabled = false;
                this.levelUpDown.Enabled = false;
                this.comboBox1.Enabled = false;
                this.heightUpDown.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Properties.Resources.InvalidPort + " " + portUpDown.Value);
            }

            this.progressBar1.Value = this.progressBar1.Minimum;
        }

        private int tries;

        /// <summary>
        /// Timer's tick event handler. If no connection was estabilished after 30 ticks, times out.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.tries < 300)
            {
                this.progressBar1.Value = (int)((this.tries + 1) * (this.progressBar1.Maximum - this.progressBar1.Minimum) / 300.0);
                this.tries++;

                if (this.host.Pending())
                {
                    this.Client = this.host.AcceptTcpClient();
                    this.timer1.Enabled = false;
                    this.button1.Enabled = true;
                    this.portUpDown.Enabled = true;
                    this.levelUpDown.Enabled = true;
                    this.comboBox1.Enabled = true;
                    this.heightUpDown.Enabled = true;
                    this.Level = (uint) this.levelUpDown.Value;
                    this.BMode = (this.comboBox1.SelectedIndex == 1);
                    this.BHeight = (this.BMode) ? (uint) this.heightUpDown.Value: 0;
                    this.DialogResult = DialogResult.OK;
                }
            }
            else
            {
                this.timer1.Enabled = false;
                this.button1.Enabled = true;
                this.portUpDown.Enabled = true;
                this.levelUpDown.Enabled = true;
                this.comboBox1.Enabled = true;
                this.heightUpDown.Enabled = true;
                this.progressBar1.Value = 0;
                this.host.Stop();
                MessageBox.Show(Properties.Resources.Timeout);
            }
        }

        /// <summary>
        /// Form closing event handler. Closes the socket, if open.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HostForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.host != null)
                this.host.Stop();
            this.Dispose();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 1)
                heightUpDown.Enabled = true;
            else
                heightUpDown.Enabled = false;
        }
    }
}
