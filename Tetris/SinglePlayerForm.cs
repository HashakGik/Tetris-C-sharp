using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// Single player form.
    /// </summary>
    public partial class SinglePlayerForm : Form
    {
        private Tetris t;
        /// <summary>
        /// Key bindings data structure.
        /// </summary>
        private Dictionary<string, string> keys;
        private Palette palette;
        private bool movingLeft;
        private bool movingRight;
        private bool movingDown;
        private bool paused;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="level">Initial level.</param>
        /// <param name="bMode">Playing mode. True for mode B, false for mode A.</param>
        /// <param name="bHeight">Initial height in mode B.</param>
        public SinglePlayerForm(uint level, bool bMode = false, uint bHeight = 0)
        {
            InitializeComponent();

            this.movingLeft = false;
            this.movingRight = false;
            this.movingDown = false;
            this.paused = false;

            // Reflection used to add DoubleBuffered property on Panel instances.
            typeof(Panel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, panel1, new object[] { true });
            typeof(Panel).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, panel2, new object[] { true });

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

            // Populate the key bindings data structure.
            for (int i = 0; i < 6; i++)
            {
                foreach (string s in props[i, 1].Split(';'))
                    if (s.Length > 0)
                        this.keys[s] = props[i, 0];
            }


            t = new Tetris(10, 20, level, bMode, bHeight); // The playing field is initialized to the NES values (10 blocks width, 20 blocks height).
            this.Text = Properties.Resources.SingleTitle;

            // If in Mode B attaches the victory delegate.
            if (bMode)
            {
                this.Text += " " + Properties.Resources.ModeBTitle;
                t.OnVictory += delegate () { timer1.Enabled = false; timer2.Enabled = false; MessageBox.Show("You won" + Environment.NewLine + "Score: " + t.Score); this.Dispose(); };
            }
            else
                this.Text += " " + Properties.Resources.ModeATitle;


            t.OnGameOver += delegate () { timer1.Enabled = false; timer2.Enabled = false; MessageBox.Show("Game over" + Environment.NewLine + "Score: " + t.Score); this.Dispose(); }; // TO DO: Save the best scores.
            t.OnLevelUp += delegate (uint lv) { timer1.Interval = (int)((lv < 20) ? 1000 / (lv + 1) : 50); }; // On level up it speeds up the timer. Maximum speed is achieved on level 20.

            // It fires the initial level up to set the correct timer interval.
            t.OnLevelUp(t.Level);
        }

        /// <summary>
        /// Game panel paint event handler. It paints the playing field, unless paused.
        /// Each block is painted according to its two bits and according to the current level.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Bitmap bmpA = new Bitmap(Properties.Resources.A);
            Bitmap bmpB = new Bitmap(Properties.Resources.B);
            List<BitArray> b;

            b = t.GetImage();

            e.Graphics.Clear(Color.Black);

            if (this.paused)
            {
                StringFormat f = new StringFormat();
                f.LineAlignment = StringAlignment.Center;
                f.Alignment = StringAlignment.Center;

                e.Graphics.DrawString(Properties.Resources.Paused, DefaultFont, Brushes.White, panel1.Width / 2, panel1.Height / 2, f);
            }
            else
                for (int i = 0; i < 10; i++)
                    for (int j = 0; j < 20; j++)
                    {
                        if (b[j][2 * i] | b[j][2 * i + 1])
                        {
                            e.Graphics.FillRectangle(this.palette.Get(t.Level, b[j][2 * i]), 20 * i, 19 * 20 - 20 * j, 20, 20);
                            if (b[j][2 * i + 1])
                                e.Graphics.DrawImage(bmpB, 20 * i, 19 * 20 - 20 * j);
                            else
                                e.Graphics.DrawImage(bmpA, 20 * i, 19 * 20 - 20 * j);
                        }

#if DEBUG
                        // For debug purposes show a grid over the field.
                        e.Graphics.DrawRectangle(Pens.White, panel1.Width / 10 * i, panel1.Height / 20 * j, panel1.Width / 10, panel1.Height / 20);
#endif
                    }
        }
        /// <summary>
        /// Game timer's tick. It updates the game and repaints the panels.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            t.Update();
            panel1.Invalidate();
            panel2.Invalidate();
        }
        /// <summary>
        /// Input timer's tick. If a key has been pressed down, but not yet up, it moves the current tetromino accordingly.
        /// This timer's interval is not influenced by the current level.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (movingLeft)
            {
                t.MoveLeft();
                panel1.Invalidate();
            }

            if (movingRight)
            {
                t.MoveRight();
                panel1.Invalidate();
            }
            if (movingDown)
            {
                t.MoveDown();
                panel1.Invalidate();
                panel2.Invalidate();
            }
        }
        /// <summary>
        /// Keyboard key down event handler. It activates the continuous firing of MoveLeft(), MoveRight() and MoveDown() through timer2 and pauses the game.
        /// The key bindings are decoded from the dictionary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.keys.ContainsKey(e.KeyData.ToString()))
            {
                if (this.keys[e.KeyData.ToString()] == "Left" && !this.paused)
                    movingLeft = true;
                if (this.keys[e.KeyData.ToString()] == "Right" && !this.paused)
                    movingRight = true;
                if (this.keys[e.KeyData.ToString()] == "Drop" && !this.paused)
                {
                    movingDown = true;
                    t.Dropping = true;
                }
                if (this.keys[e.KeyData.ToString()] == "Pause")
                {
                    this.paused = !this.paused;
                    timer1.Enabled = !this.paused;
                    timer2.Enabled = !this.paused;
                    if (this.paused)
                    {
                        movingDown = false;
                        movingLeft = false;
                        movingRight = false;
                    }
                    panel1.Invalidate();
                    panel2.Invalidate();
                }
            }
        }
        /// <summary>
        /// Keyboard key up event handler. It deactivates the continuous firing of MoveLeft(), MoveRight() and MoveDown() through timer2 and fires RotateLeft() and RotateRight().
        /// The key bindings are decoded from the dictionary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.keys.ContainsKey(e.KeyData.ToString()))
            {
                if (this.keys[e.KeyData.ToString()] == "RotateLeft" && !this.paused)
                {
                    t.RotateLeft();
                    panel1.Invalidate();
                }
                if (this.keys[e.KeyData.ToString()] == "RotateRight" && !this.paused)
                {
                    t.RotateRight();
                    panel1.Invalidate();
                }


                if (this.keys[e.KeyData.ToString()] == "Left")
                    movingLeft = false;
                if (this.keys[e.KeyData.ToString()] == "Right")
                    movingRight = false;
                if (this.keys[e.KeyData.ToString()] == "Drop")
                {
                    movingDown = false;
                    t.Dropping = false;
                }
            }
        }

        private void SinglePlayerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Dispose(); // BUG: this stops the timers but WON'T deallocate the form. Starting repeatedly a single player game may cause a memory leak.
        }

        /// <summary>
        /// Statistics panel's paint event handler. It displays the current level, score and cleared lines, the next tetromino and the statistics about the dropped tetrominoes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            Tetromino[] t = new Tetromino[7];
            for (int i = 0; i < 7; i++)
                t[i] = new Tetromino((Tetromino.Type_t)i);

            Bitmap bmpA = new Bitmap(new Bitmap(Properties.Resources.A), 10, 10);
            Bitmap bmpB = new Bitmap(new Bitmap(Properties.Resources.B), 10, 10);
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 4; j++)
                {
                    e.Graphics.FillRectangle(this.palette.Get(this.t.Level, t[i].Appearance[0]), 20 + t[i].Get()[j].X * 10, i * 40 + t[i].Get()[j].Y * 10 + 30, 10, 10);
                    e.Graphics.DrawImage((t[i].Appearance[1]) ? bmpB : bmpA, 20 + t[i].Get()[j].X * 10, i * 40 + t[i].Get()[j].Y * 10 + 30);
                }
            for (int i = 0; i < 7; i++)
                e.Graphics.DrawString(this.t.Statistics[i].ToString(), DefaultFont, Brushes.Black, 55, 40 * i + 30);
            e.Graphics.DrawString(Properties.Resources.Next, DefaultFont, Brushes.Black, 20, 350);
            if (!this.paused)
                for (int j = 0; j < 4; j++)
                {
                    e.Graphics.FillRectangle(this.palette.Get(this.t.Level, t[(int)this.t.Next].Appearance[0]), 20 + t[(int)this.t.Next].Get()[j].X * 10, 370 + t[(int)this.t.Next].Get()[j].Y * 10, 10, 10);
                    e.Graphics.DrawImage((t[(int)this.t.Next].Appearance[1]) ? bmpB : bmpA, 20 + t[(int)this.t.Next].Get()[j].X * 10, 370 + t[(int)this.t.Next].Get()[j].Y * 10);
                }

            StringFormat f = new StringFormat();
            f.LineAlignment = StringAlignment.Center;
            f.Alignment = StringAlignment.Center;

            e.Graphics.DrawString(Properties.Resources.Level + Environment.NewLine + this.t.Level, DefaultFont, Brushes.Black, 100, 40, f);
            e.Graphics.DrawString(Properties.Resources.Lines + Environment.NewLine + this.t.Lines, DefaultFont, Brushes.Black, 100, 80, f);
            e.Graphics.DrawString(Properties.Resources.Score + Environment.NewLine + this.t.Score, DefaultFont, Brushes.Black, 100, 120, f);
        }
    }
}
