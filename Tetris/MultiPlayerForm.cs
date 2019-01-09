using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// Multiplayer form. Most elements work like their counterpart in SinglePlayerForm.
    /// </summary>
    public partial class MultiPlayerForm : Form
    {
        private bool localGameOver;
        private bool remoteGameOver;
        private bool movingLeft;
        private bool movingRight;
        private bool movingDown;
        private NetworkStream stream;
        private TcpClient client;
        private Palette palette;
        private Dictionary<string, string> keys;
        /// <summary>
        /// Two Tetris instances are used: one for the local game and the other for the opponent's game.
        /// </summary>
        private NetworkTetris t1, t2;

        public MultiPlayerForm(TcpClient t, uint level, bool host, bool bMode = false, uint bHeight = 0)
        {
            InitializeComponent();

            this.movingLeft = false;
            this.movingRight = false;
            this.movingDown = false;

            // Reflection used to add DoubleBuffered property on Panel instances.
            typeof(Panel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, panel1, new object[] { true });
            typeof(Panel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, panel2, new object[] { true });
            typeof(Panel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, panel3, new object[] { true });

            this.keys = new Dictionary<string, string>();
            this.palette = new Palette();

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
                foreach (string s in props[i, 1].Split(';'))
                    if (s.Length > 0)
                        this.keys[s] = props[i, 0];
            }


            client = t;
            stream = client.GetStream();

            t1 = new NetworkTetris(10, 20, stream, true, host, level, bMode, bHeight); // Transmitter: the form events control this instance and the updates are sent to the opponent.
            t2 = new NetworkTetris(10, 20, stream, false, host, level, bMode, bHeight); // Receiver: the network stream updates this instance with the opponent moves.

            if (!host)
                bMode = t1.BMode; // If the player is not the host, update the chosen mode.

            this.localGameOver = false;
            this.remoteGameOver = false;
            t1.OnGameOver += delegate () { localGameOver = true; CheckGameOver(); };
            t1.OnVictory += delegate () { localGameOver = true; CheckGameOver(); /* TO DO: add bonus points if the player wins first. */ };
            t1.OnLevelUp += delegate (uint lv) { timer1.Interval = (int)((lv < 20) ? 1000 / (lv + 1): 50); };
            t1.OnLevelUp(t1.Level);

            t2.OnGameOver += delegate () { timer3.Enabled = false; remoteGameOver = true; CheckGameOver(); };
            t2.OnVictory += delegate () { remoteGameOver = true; CheckGameOver(); /* TO DO: add bonus points to the opponent if the opponent wins first. */ };
            t1.OnConnectionError += delegate () { timer1.Enabled = false; timer2.Enabled = false; timer3.Enabled = false; if (!this.localGameOver || !this.remoteGameOver) MessageBox.Show("Connection lost"); };
            t2.OnConnectionError += delegate () { timer1.Enabled = false; timer2.Enabled = false; timer3.Enabled = false; if (!this.localGameOver || !this.remoteGameOver) MessageBox.Show("Connection lost"); };

            this.Text = Properties.Resources.MultiTitle;
            if (bMode)
                this.Text += " " + Properties.Resources.ModeBTitle;
            else
                this.Text += " " + Properties.Resources.ModeATitle;
        }
        /// <summary>
        /// Checks if both players have lost and displays the final scores.
        /// </summary>
        private void CheckGameOver()
        {
            if (this.localGameOver && this.remoteGameOver)
            {
                MessageBox.Show(Properties.Resources.GameOverM + Environment.NewLine + Properties.Resources.PlayerScore + ": " + t1.Score + Environment.NewLine + Properties.Resources.OpponentScore + ": " + t2.Score);
                // TO DO: Save the best scores.

               // this.Close(); // Close() can't be invoked from a different thread than the one the form is running.
            }
        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            Tetromino[] t = new Tetromino[7];
            for (int i = 0; i < 7; i++)
                t[i] = new Tetromino((Tetromino.Type_t)i);

            Bitmap bmpA = new Bitmap(new Bitmap(Properties.Resources.A), 10, 10);
            Bitmap bmpB = new Bitmap(new Bitmap(Properties.Resources.B), 10, 10);

            Bitmap bmp = new Bitmap(new Bitmap(Properties.Resources.A), 10, 10);
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 4; j++)
                {
                    e.Graphics.FillRectangle(this.palette.Get(this.t1.Level, t[i].Appearance[0]), 75 + t[i].Get()[j].X * 10, i * 40 + t[i].Get()[j].Y * 10 + 30, 10, 10);
                    e.Graphics.DrawImage((t[i].Appearance[1]) ? bmpB : bmpA, 75 + t[i].Get()[j].X * 10, i * 40 + t[i].Get()[j].Y * 10 + 30);
                }
            for (int i = 0; i < 7; i++)
            {
                e.Graphics.DrawString(this.t1.Statistics[i].ToString(), DefaultFont, Brushes.Black, 40, 40 * i + 30);
                e.Graphics.DrawString(this.t2.Statistics[i].ToString(), DefaultFont, Brushes.Black, 100, 40 * i + 30);
            }
                
            e.Graphics.DrawString(Properties.Resources.Next, DefaultFont, Brushes.Black, 70, 355);
            for (int j = 0; j < 4; j++)
            {
                e.Graphics.FillRectangle(this.palette.Get(this.t1.Level, t[(int)this.t1.Next].Appearance[0]), 75 + t[(int)this.t1.Next].Get()[j].X * 10, 370 + t[(int)this.t1.Next].Get()[j].Y * 10, 10, 10);
                e.Graphics.DrawImage((t[(int)this.t1.Next].Appearance[1]) ? bmpB : bmpA, 75 + t[(int)this.t1.Next].Get()[j].X * 10, 370 + t[(int)this.t1.Next].Get()[j].Y * 10);
            }
            
            StringFormat f = new StringFormat();
            f.LineAlignment = StringAlignment.Center;
            f.Alignment = StringAlignment.Center;

            e.Graphics.DrawString(Properties.Resources.Level + Environment.NewLine + this.t1.Level, DefaultFont, Brushes.Black, 15, 40, f);
            e.Graphics.DrawString(Properties.Resources.Lines + Environment.NewLine + this.t1.Lines, DefaultFont, Brushes.Black, 15, 80, f);
            e.Graphics.DrawString(Properties.Resources.Score + Environment.NewLine + this.t1.Score, DefaultFont, Brushes.Black, 15, 120, f);
            e.Graphics.DrawString(Properties.Resources.Level + Environment.NewLine + this.t2.Level, DefaultFont, Brushes.Black, 135, 40, f);
            e.Graphics.DrawString(Properties.Resources.Lines + Environment.NewLine + this.t2.Lines, DefaultFont, Brushes.Black, 135, 80, f);
            e.Graphics.DrawString(Properties.Resources.Score + Environment.NewLine + this.t2.Score, DefaultFont, Brushes.Black, 135, 120, f);
        }

        /// <summary>
        /// Opponent field panel's paint event handler. Works like the player field panel's paint.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            Bitmap bmpA = new Bitmap(Properties.Resources.A);
            Bitmap bmpB = new Bitmap(Properties.Resources.B);
            List<BitArray> b;

            b = t2.GetImage();

            e.Graphics.Clear(Color.Black);


            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 20; j++)
                {
                    if (b[j][2 * i] | b[j][2 * i + 1])
                    {
                        e.Graphics.FillRectangle(this.palette.Get(t2.Level, b[j][2 * i]), 20 * i, 19 * 20 - 20 * j, 20, 20);
                        if (b[j][2 * i + 1])
                            e.Graphics.DrawImage(bmpB, 20 * i, 19 * 20 - 20 * j);
                        else
                            e.Graphics.DrawImage(bmpA, 20 * i, 19 * 20 - 20 * j);
                    }

#if DEBUG
                    e.Graphics.DrawRectangle(Pens.White, panel1.Width / 10 * i, panel1.Height / 20 * j, panel1.Width / 10, panel1.Height / 20);
#endif
                }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!localGameOver)
            {
                t1.Update();
                panel1.Invalidate();
                panel2.Invalidate();
            }
            else
                t1.Serialize(); // Continues transmitting even after the game is lost to prevent the closing of the socket.
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (!localGameOver)
            {
                if (movingLeft)
                {
                    t1.MoveLeft();
                    panel1.Invalidate();
                }

                if (movingRight)
                {
                    t1.MoveRight();
                    panel1.Invalidate();
                }
                if (movingDown)
                {
                    t1.MoveDown();
                    panel1.Invalidate();
                    panel2.Invalidate();
                }
            }
        }
        private void MultiPlayerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // TO DO: if the game is already lost, it should prevent the closing of the socket in order to avoid "cheating".

            try
            {
                if (!this.localGameOver)
                    this.t1.OnGameOver();
            }
            catch (Exception ex)
            {
                t1.OnConnectionError();
            }

            t2.Stop();

            stream.Close();
            client.Close();

            this.Dispose();
        }
        private void MultiPlayerForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.keys.ContainsKey(e.KeyData.ToString()))
            {
                if (this.keys[e.KeyData.ToString()] == "Left")
                    movingLeft = true;
                if (this.keys[e.KeyData.ToString()] == "Right")
                    movingRight = true;
                if (this.keys[e.KeyData.ToString()] == "Drop")
                {
                    movingDown = true;
                    t1.Dropping = true;
                }
            }
        }
        private void MultiPlayerForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.keys.ContainsKey(e.KeyData.ToString()))
            {
                if (this.keys[e.KeyData.ToString()] == "RotateLeft")
                {
                    t1.RotateLeft();
                    panel1.Invalidate();
                }
                if (this.keys[e.KeyData.ToString()] == "RotateRight")
                {
                    t1.RotateRight();
                    panel1.Invalidate();
                }


                if (this.keys[e.KeyData.ToString()] == "Left")
                    movingLeft = false;
                if (this.keys[e.KeyData.ToString()] == "Right")
                    movingRight = false;
                if (this.keys[e.KeyData.ToString()] == "Drop")
                {
                    movingDown = false;
                    t1.Dropping = false;
                }
            }
        }
        private void timer3_Tick(object sender, EventArgs e)
        {
            if (!remoteGameOver)
            {
                panel2.Invalidate();
                panel3.Invalidate();
            }
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

            Bitmap bmpA = new Bitmap(Properties.Resources.A);
            Bitmap bmpB = new Bitmap(Properties.Resources.B);
            List<BitArray> b;

            b = t1.GetImage();

            e.Graphics.Clear(Color.Black);


            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 20; j++)
                {
                    if (b[j][2 * i] | b[j][2 * i + 1])
                    {
                        e.Graphics.FillRectangle(this.palette.Get(t1.Level, b[j][2 * i]), 20 * i, 19 * 20 - 20 * j, 20, 20);
                        if (b[j][2 * i + 1])
                            e.Graphics.DrawImage(bmpB, 20 * i, 19 * 20 - 20 * j);
                        else
                            e.Graphics.DrawImage(bmpA, 20 * i, 19 * 20 - 20 * j);
                    }
#if DEBUG
                    e.Graphics.DrawRectangle(Pens.White, panel1.Width / 10 * i, panel1.Height / 20 * j, panel1.Width / 10, panel1.Height / 20);
#endif
                }
        }
    }
}
