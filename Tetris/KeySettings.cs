using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// Key bindings form. For each command it allows any number of keys. If a key is already bound to another command, it's reassigned to the current command.
    /// </summary>
    public partial class KeySettings : Form
    {
        private bool left, right, rLeft, rRight, drop, pause;
        private Dictionary<string, string> keys;

        private void button3_Click(object sender, EventArgs e)
        {
            this.rLeft = true;

            this.button3.Text = Properties.Resources.EscToFinish;
            this.button1.Enabled = false;
            this.button2.Enabled = false;
            this.button3.Enabled = false;
            this.button4.Enabled = false;
            this.button5.Enabled = false;
            this.button6.Enabled = false;
            this.button7.Enabled = false;
            this.button8.Enabled = false;
            this.button9.Enabled = false;
            this.button3.Select();
            this.AcceptButton = null;
            this.CancelButton = null;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            this.rRight = true;

            this.button4.Text = Properties.Resources.EscToFinish;
            this.button1.Enabled = false;
            this.button2.Enabled = false;
            this.button3.Enabled = false;
            this.button4.Enabled = false;
            this.button5.Enabled = false;
            this.button6.Enabled = false;
            this.button7.Enabled = false;
            this.button8.Enabled = false;
            this.button9.Enabled = false;
            this.button4.Select();
            this.AcceptButton = null;
            this.CancelButton = null;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            this.drop = true;

            this.button5.Text = Properties.Resources.EscToFinish;
            this.button1.Enabled = false;
            this.button2.Enabled = false;
            this.button3.Enabled = false;
            this.button4.Enabled = false;
            this.button5.Enabled = false;
            this.button6.Enabled = false;
            this.button7.Enabled = false;
            this.button8.Enabled = false;
            this.button9.Enabled = false;
            this.button5.Select();
            this.AcceptButton = null;
            this.CancelButton = null;
        }
        private void button6_Click(object sender, EventArgs e)
        {
            this.pause = true;

            this.button6.Text = Properties.Resources.EscToFinish;
            this.button1.Enabled = false;
            this.button2.Enabled = false;
            this.button3.Enabled = false;
            this.button4.Enabled = false;
            this.button5.Enabled = false;
            this.button6.Enabled = false;
            this.button7.Enabled = false;
            this.button8.Enabled = false;
            this.button9.Enabled = false;
            this.button6.Select();
            this.AcceptButton = null;
            this.CancelButton = null;
        }
        private void KeySettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                this.left = false;
                this.right = false;
                this.rLeft = false;
                this.rRight = false;
                this.drop = false;
                this.pause = false;
                this.button1.Text = Properties.Resources.Left;
                this.button2.Text = Properties.Resources.Right;
                this.button3.Text = Properties.Resources.RotateLeft;
                this.button4.Text = Properties.Resources.RotateRight;
                this.button5.Text = Properties.Resources.Drop;
                this.button6.Text = Properties.Resources.Pause;
                this.button1.Enabled = true;
                this.button2.Enabled = true;
                this.button3.Enabled = true;
                this.button4.Enabled = true;
                this.button5.Enabled = true;
                this.button6.Enabled = true;
                this.button7.Enabled = true;
                this.button8.Enabled = true;
                this.button9.Enabled = true;
                this.AcceptButton = this.button7;
                this.CancelButton = this.button8;
            }
            else
            {
                if (this.left)
                {
                    this.keys[e.KeyData.ToString()] = "Left";
                }
                if (this.right)
                {
                    this.keys[e.KeyData.ToString()] = "Right";
                }
                if (this.rLeft)
                {
                    this.keys[e.KeyData.ToString()] = "RotateLeft";
                }
                if (this.rRight)
                {
                    this.keys[e.KeyData.ToString()] = "RotateRight";
                }
                if (this.drop)
                {
                    this.keys[e.KeyData.ToString()] = "Drop";
                }
                if (this.pause)
                {
                    this.keys[e.KeyData.ToString()] = "Pause";
                }
            }



            label1.Text = "";
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";
            label5.Text = "";
            label6.Text = "";

            foreach (KeyValuePair<string, string> p in this.keys)
                switch (p.Value)
                {
                    case "Left":
                        label1.Text += p.Key + ";";
                        break;
                    case "Right":
                        label2.Text += p.Key + ";";
                        break;
                    case "RotateLeft":
                        label3.Text += p.Key + ";";
                        break;
                    case "RotateRight":
                        label4.Text += p.Key + ";";
                        break;
                    case "Drop":
                        label5.Text += p.Key + ";";
                        break;
                    case "Pause":
                        label6.Text += p.Key + ";";
                        break;
                }
        }
        private void button9_Click(object sender, EventArgs e)
        {
            this.keys.Clear();
            label1.Text = "";
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";
            label5.Text = "";
            label6.Text = "";
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.right = true;

            this.button2.Text = Properties.Resources.EscToFinish;
            this.button1.Enabled = false;
            this.button2.Enabled = false;
            this.button3.Enabled = false;
            this.button4.Enabled = false;
            this.button5.Enabled = false;
            this.button6.Enabled = false;
            this.button7.Enabled = false;
            this.button8.Enabled = false;
            this.button9.Enabled = false;
            this.button2.Select();
            this.AcceptButton = null;
            this.CancelButton = null;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.left = true;

            this.button1.Text = Properties.Resources.EscToFinish;
            this.button1.Enabled = false;
            this.button2.Enabled = false;
            this.button3.Enabled = false;
            this.button4.Enabled = false;
            this.button5.Enabled = false;
            this.button6.Enabled = false;
            this.button7.Enabled = false;
            this.button8.Enabled = false;
            this.button9.Enabled = false;
            this.button1.Select();
            this.AcceptButton = null;
            this.CancelButton = null;
        }
        /// <summary>
        /// Constructor. Reads the stored key bindings.
        /// </summary>
        public KeySettings()
        {
            InitializeComponent();
            this.left = this.right = this.rRight = this.rLeft = this.drop = this.pause = false;
            this.keys = new Dictionary<string, string>();

            label1.Text = "";
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";
            label5.Text = "";
            label6.Text = "";

            // A foreach on Properties.KeyMapping.Default.PropertyValues doesn't always work, so it's better to read manually every property.
            string[,] props = new string[6, 2];
            props[0, 0] = "Left";
            props[1, 0] = "Right";
            props[2, 0] = "RotateLeft";
            props[3, 0] = "RotateRight";
            props[4, 0] = "Drop";
            props[5, 0] = "Pause";
            props[0, 1] = Properties.KeyMapping.Default.Left;
            props[1, 1] = Properties.KeyMapping.Default.Right;
            props[2, 1] = Properties.KeyMapping.Default.RotateLeft;
            props[3, 1] = Properties.KeyMapping.Default.RotateRight;
            props[4, 1] = Properties.KeyMapping.Default.Drop;
            props[5, 1] = Properties.KeyMapping.Default.Pause;

            for (int i = 0; i < 6; i++)
            {
                foreach (string s in props[i,1].Split(';'))
                    if (s.Length > 0)
                        this.keys[s] = props[i,0];
            }

            foreach (KeyValuePair<string, string> p in this.keys)
                switch (p.Value)
                {
                    case "Left":
                        label1.Text += p.Key + ";";
                        break;
                    case "Right":
                        label2.Text += p.Key + ";";
                        break;
                    case "RotateLeft":
                        label3.Text += p.Key + ";";
                        break;
                    case "RotateRight":
                        label4.Text += p.Key + ";";
                        break;
                    case "Drop":
                        label5.Text += p.Key + ";";
                        break;
                    case "Pause":
                        label6.Text += p.Key + ";";
                        break;
                }

            this.button1.Text = Properties.Resources.Left;
            this.button2.Text = Properties.Resources.Right;
            this.button3.Text = Properties.Resources.RotateLeft;
            this.button4.Text = Properties.Resources.RotateRight;
            this.button5.Text = Properties.Resources.Drop;
            this.button6.Text = Properties.Resources.Pause;
            this.button7.Text = Properties.Resources.Accept;
            this.button8.Text = Properties.Resources.Cancel;
            this.button9.Text = Properties.Resources.Clear;
            this.Text = Properties.Resources.SettingsTitle;

        }
        private void button7_Click(object sender, EventArgs e)
        {
            bool ok = true;

            string str = "";
            if (label1.Text == "")
            {
                str += " Left";
                ok = false;
            }
            if (label2.Text == "")
            {
                str += " Right";
                ok = false;
            }
            if (label3.Text == "")
            {
                str += " RotateLeft";
                ok = false;
            }
            if (label4.Text == "")
            {
                str += " RotateRight";
                ok = false;
            }
            if (label5.Text == "")
            {
                str += " Drop";
                ok = false;
            }
            if (label6.Text == "")
            {
                str += " Pause";
                ok = false;
            }



            if (ok)
            {
                Properties.KeyMapping.Default.Left = "";
                Properties.KeyMapping.Default.Right = "";
                Properties.KeyMapping.Default.RotateLeft = "";
                Properties.KeyMapping.Default.RotateRight = "";
                Properties.KeyMapping.Default.Drop = "";
                Properties.KeyMapping.Default.Pause = "";

                foreach (KeyValuePair<string, string> p in this.keys)
                    switch (p.Value)
                    {
                        case "Left":
                            Properties.KeyMapping.Default.Left += p.Key + ";";
                            break;
                        case "Right":
                            Properties.KeyMapping.Default.Right += p.Key + ";";
                            break;
                        case "RotateLeft":
                            Properties.KeyMapping.Default.RotateLeft += p.Key + ";";
                            break;
                        case "RotateRight":
                            Properties.KeyMapping.Default.RotateRight += p.Key + ";";
                            break;
                        case "Drop":
                            Properties.KeyMapping.Default.Drop += p.Key + ";";
                            break;
                        case "Pause":
                            Properties.KeyMapping.Default.Pause += p.Key + ";";
                            break;
                    }

                Properties.KeyMapping.Default.Save();
                this.Close();
            }
            else
                MessageBox.Show(Properties.Resources.NotSet + ":" + str);
        }
    }
}
