using System;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// Initial form. It simply opens the other forms, passing parameters between them.
    /// </summary>
    public partial class StartForm : Form
    {
        public StartForm()
        {
            InitializeComponent();

            this.button1.Text = Properties.Resources.SinglePlayer;
            this.button2.Text = Properties.Resources.HostGame;
            this.button3.Text = Properties.Resources.JoinGame;
            this.button5.Text = Properties.Resources.Settings;
            this.Text = Properties.Resources.StartTitle;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            LevelForm f = new LevelForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                Form f2 = new SinglePlayerForm(f.Level, f.BMode, ((f.BMode)? f.BHeight : 0));
                f.Close(); // In order to avoid parameters being garbage collected, the first form must be closed AFTER the second is created.
                f2.ShowDialog();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            HostForm f = new HostForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                MultiPlayerForm f2 = new MultiPlayerForm(f.Client, f.Level, true, f.BMode, f.BHeight);
                f.Close();
                f2.ShowDialog();
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            JoinForm f = new JoinForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                MultiPlayerForm f2 = new MultiPlayerForm(f.Client, 1, false);
                f.Close();
                f2.ShowDialog();
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            Form f = new KeySettings();
            if (f.ShowDialog() == DialogResult.OK)
                Properties.KeyMapping.Default.Reload();
        }
    }
}
