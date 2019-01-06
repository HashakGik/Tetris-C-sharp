using System;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// Level selection form for the single player mode. It stores the user's choices in properties which will be read by the parent form.
    /// </summary>
    public partial class LevelForm : Form
    {
        /// <summary>
        /// Chosen level.
        /// </summary>
        public uint Level { get; set; }
        /// <summary>
        /// Initial height in mode B.
        /// </summary>
        public uint BHeight { get; set; }
        /// <summary>
        /// Game mode (true: mode B, false: mode A).
        /// </summary>
        public bool BMode { get; set; }

        public LevelForm()
        {
            InitializeComponent();

            this.comboBox1.SelectedIndex = 0;
            this.label1.Text = Properties.Resources.Level;
            this.label2.Text = Properties.Resources.HeightString;
            this.label3.Text = Properties.Resources.ModeString;
            this.Text = Properties.Resources.LevelTitle;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Level = (uint) numericUpDown1.Value;
            this.BHeight = (uint)numericUpDown2.Value;
            this.BMode = (this.comboBox1.SelectedIndex == 1);
            this.DialogResult = DialogResult.OK;
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 1)
                this.numericUpDown2.Enabled = true;
            else
                this.numericUpDown2.Enabled = false;
        }
    }
}
