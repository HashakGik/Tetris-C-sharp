using System;
using System.Collections;
using System.Collections.Generic;

namespace Tetris
{
    /// <summary>
    /// Tetris game class. It implements the NES rule set:
    /// - Only tetrominoes are allowed (every piece has always four connected blocks and therefore there are only seven possible pieces)
    /// - Soft dropping (pressing the drop button speeds up the falling of the tetromino, but won't force instant landing. A bonus score is given if the tetromino lands during soft drop)
    /// - No wall kick (if there is a lateral obstacle, the tetromino can't rotate), but the tetromino is allowed to rotate even if it overflows the upper side of the field
    /// - A level up is triggered once the number of lines cleared reaches ten times the current level plus one
    /// - Mode B is won by clearing 25 lines. The initial height determines how many random blocks are placed at the beginning
    /// - No recursive gravity (after a line is cleared, every block will fall together, empty spaces are not filled by detached blocks)
    /// - The score depends only on the number of consecutive lines cleared and the current level (since there is no recursive gravity, there can't be more than four consecutive lines cleared).
    /// </summary>
    class Tetris
    {
        public delegate void GameOver();
        public delegate void LevelUp(uint level);
        protected int w;
        protected int h;
        protected List<BitArray> field; // field[0][0..2w] = bottom, field[h][0..2w] = top
        protected Tetromino current, next;
        protected int x, y;
        protected bool bMode;
        protected uint bHeight;
        protected Random r;
        private int consecutiveLinesCleared;
        private int droppingHeight;

        /// <summary>
        /// Game over delegate.
        /// </summary>
        public GameOver OnGameOver
        {
            get; set;
        }
        /// <summary>
        /// Victory delegate.
        /// </summary>
        public GameOver OnVictory
        {
            get; set;
        }
        /// <summary>
        /// Level up delegate.
        /// </summary>
        public LevelUp OnLevelUp
        {
            get; set;
        }
        public long Score
        {
            get; set;
        }
        public uint Level
        {
            get; set;
        }
        public bool Dropping
        {
            get; set;
        }
        public int[] Statistics
        {
            get;
        }
        public int Lines
        {
            get; set;
        }
        public Tetromino.Type_t Current
        {
            get
            { return this.current.Type; }
        }
        public Tetromino.Type_t Next
        {
            get
            { return this.next.Type; }
        }

        public bool BMode
        {
            get
            { return this.bMode; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="w">Width of the playing field.</param>
        /// <param name="h">Height of the playing field.</param>
        /// <param name="level">Initial level.</param>
        /// <param name="bMode">Playing mode. True: mode B, false: mode A.</param>
        /// <param name="bHeight">Initial height in mode B.</param>
        public Tetris(int w, int h, uint level = 1, bool bMode = false, uint bHeight = 0)
        {
            this.w = w;
            this.h = h + 2; // Adds an overflow area to allow the rotation of the tetromino even when it's at the top of the screen.
            this.x = w / 2 - 1;
            this.y = this.h - 3;
            this.field = new List<BitArray>(this.h);
            for (int i = 0; i < this.h; i++)
            {
                this.field.Add(new BitArray(2 * w));
            }
            this.Statistics = new int[7];
            for (int i = 0; i < 7; i++)
                this.Statistics[i] = 0;


            this.r = new Random(Guid.NewGuid().GetHashCode()); // There may be situations in which more Tetris instances are created simultaneously and therefore initialized with the same seed (e.g. when playing a multiplayer game on localhost), so it's required to guarantee a unique seed.
            this.current = new Tetromino((Tetromino.Type_t) r.Next(7));
            this.Statistics[(int)this.current.Type]++;

            this.next = new Tetromino((Tetromino.Type_t)r.Next(7));
            this.Score = 0;
            this.Level = level;
            this.Lines = 0;
            this.consecutiveLinesCleared = 0;
            this.Dropping = false;
            this.droppingHeight = 0;
            this.bMode = bMode;
            this.bHeight = bHeight;

            if (this.bMode)
            {
                this.Lines = 25;
                // In mode B populates the field with a number of blocks between 6% and 7% of the playing field (excluding the overflow area on top) multiplied by the chosen height.
                int numBlocks = (int) (this.w * h * this.bHeight * (this.r.NextDouble() + 6) / 100.0);
                int tmp;

                for (int i = 0, k = 0; i < this.h; i++)
                    for (int j = 0; j < this.w; j++)
                        if (k < numBlocks && r.Next(2) == 1)
                        {
                            tmp = r.Next(3) + 1;
                            this.field[i][2 * j] = tmp % 2 != 0;
                            this.field[i][2 * j + 1] = tmp / 2 != 0;
                            
                            k++;
                        }
            }
        }

        /// <summary>
        /// Gets a data structure representing the playing field and the current tetromino.
        /// </summary>
        /// <returns>Playing field where each block is represented by two bits (see Palette class).</returns>
        virtual public List<BitArray> GetImage()
        {
            List<BitArray> ret = new List<BitArray>(this.h);
            for (int i = 0; i < this.h; i++)
                ret.Add(new BitArray(this.w * 2));

            for (int i = 0; i < this.h; i++)
                for (int j = 0; j < this.w * 2; j++)
                    ret[i][j] = this.field[i][j];

            // Add the four blocks of the current tetromino to the playing field.
            for (int i = 0; i < 4; i++)
            {
                ret[this.y - this.current.Get()[i].Y][2 * (this.x + this.current.Get()[i].X)] = this.current.Appearance[0];
                ret[this.y - this.current.Get()[i].Y][2 * (this.x + this.current.Get()[i].X) + 1] = this.current.Appearance[1];
            }

            return ret;
        }
        /// <summary>
        /// Check if the current tetromino collides with either another block or the playing field's boundaries.
        /// </summary>
        /// <returns>True if there is a collision.</returns>
        private bool CheckCollision()
        {
            bool ret = false;

            for (int i = 0; i < 4; i++)
            {
                if (this.x + this.current.Get()[i].X >= 0 && this.x + this.current.Get()[i].X < this.w &&
                        this.y - this.current.Get()[i].Y >= 0 && this.y - this.current.Get()[i].Y < this.h)
                    ret |= false;
                else
                    ret |= true;
            }
            if (!ret)
                for (int i = 0; i < 4; i++)
                    ret |= (this.field[this.y - this.current.Get()[i].Y][2 * (this.x + this.current.Get()[i].X)] | this.field[this.y - this.current.Get()[i].Y][2 * (this.x + this.current.Get()[i].X) + 1]);

            return ret;
        }
        /// <summary>
        /// Rotates the tetromino counterclockwise. If there is a collision, it rolls back to the previous state.
        /// </summary>
        virtual public void RotateLeft()
        {
            this.current.RotateLeft();
            if (this.CheckCollision())
                this.current.RotateRight();
        }
        /// <summary>
        /// Rotates the tetromino clockwise. If there is a collision, it rolls back to the previous state.
        /// </summary>
        virtual public void RotateRight()
        {
            this.current.RotateRight();
            if (this.CheckCollision())
                this.current.RotateLeft();
        }
        /// <summary>
        /// Moves the tetromino down one block. If there is a collision it has landed and it must check for lines to be cleared and generate a new tetromino.
        /// </summary>
        virtual public void MoveDown()
        {
            this.y--;

            // If it's soft dropping, it needs to keep track of the initial dropping height, in order to calculate the correct bonus score in case of landing.
            if (this.Dropping)
                this.droppingHeight++;
            else
                this.droppingHeight = 0;

            if (this.CheckCollision())
            {
                this.y++;
                // Lands the tetromino.
                for (int i = 0; i < 4; i++)
                {
                    this.field[this.y - this.current.Get()[i].Y][2 * (this.x + this.current.Get()[i].X)] = this.current.Appearance[0];
                    this.field[this.y - this.current.Get()[i].Y][2 * (this.x + this.current.Get()[i].X) + 1] = this.current.Appearance[1];
                }

                this.consecutiveLinesCleared = 0;

                // Checks which lines need to be cleared. REFACTOR: any tetromino can at most clear four lines, detecting these can reduce the number of checks.
                for (int i = 0; i < this.h;)
                    if (this.CheckLine(i))
                    {
                        this.RemoveLine(i);
                        this.consecutiveLinesCleared++;

                        // If in mode B, invokes the victory delegate if the number of remaining lines is zero.
                        if (this.bMode && this.Lines == 0 && this.OnVictory != null)
                            this.OnVictory();
                    }
                    else
                        i++;

                // If the tetromino lands during soft dropping, gives a bonus score equal to the dropping height.
                if (this.Dropping)
                {
                    this.Score += this.droppingHeight;
                    this.droppingHeight = 0;
                }

                // Generate the new tetromino.
                this.current = this.next;
                this.next = new Tetromino((Tetromino.Type_t)r.Next(7));
                this.Statistics[(int) this.current.Type]++;
                this.y = this.h - 3;
                this.x = this.w / 2 - 1;

                // Increase the score if there are lines cleared. The same NES scoring rules are applied.
                switch (this.consecutiveLinesCleared)
                {
                    case 1:
                        this.Score += 40 * (this.Level + 1);
                        break;
                    case 2:
                        this.Score += 100 * (this.Level + 1);
                        break;
                    case 3:
                        this.Score += 300 * (this.Level + 1);
                        break;
                    case 4:
                        this.Score += 1200 * (this.Level + 1);
                        break;
                }

                // Checks if the number of cleared lines is enough to invoke the level up delegate.
                if (this.bMode)
                {
                    if (25 - this.Lines >= (this.Level + 1) * 10)
                    {
                        this.Level++;
                        if (this.OnLevelUp != null)
                            this.OnLevelUp(this.Level);
                    }
                }
                else
                {
                    if (this.Lines >= (this.Level + 1) * 10)
                    {
                        this.Level++;
                        if (this.OnLevelUp != null)
                            this.OnLevelUp(this.Level);
                    }
                }
                  

            }
                
        }

        /// <summary>
        /// Moves the tetromino left. If there is a collision, it rolls back to the previous state.
        /// </summary>
        virtual public void MoveLeft()
        {
            this.x--;
            if (this.CheckCollision())
                this.x++;
        }
        /// <summary>
        /// Moves the tetromino right. If there is a collision, it rolls back to the previous state.
        /// </summary>
        virtual public void MoveRight()
        {
            this.x++;
            if (this.CheckCollision())
                this.x--;
        }

        /// <summary>
        /// Updates the game. If there is an initial collision (the current tetromino is stuck) fires the gameover, otherwise moves the tetromino downwards.
        /// </summary>
        virtual public void Update()
        {
            if (this.CheckCollision())
            {
                if (this.OnGameOver != null)
                    this.OnGameOver();
            }
            else
                this.MoveDown();
        }
        /// <summary>
        /// Checks if a line is full and needs to be cleared.
        /// </summary>
        /// <param name="i">The line to be checked.</param>
        /// <returns>True if the line is full.</returns>
        private bool CheckLine(int i)
        {
            bool ret = true;

            for (int j = 0; j < this.w; j++)
                ret &= (this.field[i][ 2 * j] | this.field[i][2 * j + 1]);

            return ret;
        }
        /// <summary>
        /// Clears a line from the field. 
        /// </summary>
        /// <param name="i">The line to be cleared.</param>
        private void RemoveLine(int i)
        {
            // It's more efficient to remove the i-th line and then add a new one on top, instead of moving every block downwards.
            this.field.RemoveAt(i);
            this.field.Add(new BitArray(this.w * 2));
            this.Lines = this.Lines + ((this.bMode) ? -1 : +1);
        }
    }
}
