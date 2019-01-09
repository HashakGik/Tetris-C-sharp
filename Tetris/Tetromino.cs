using System.Drawing;

namespace Tetris
{
    /// <summary>
    /// Tetromino data structure. Each tetromino is composed of four blocks. One of them acts as a pivot and the other three can rotate around it.
    /// The coordinates of each block are relative to the pivot.
    /// </summary>
    class Tetromino
    {
        /// <summary>
        /// Tetromino type. Encodes the seven possible pieces with a mnemonic letter resembling the shape.
        /// </summary>
        public enum Type_t { O = 0, J = 1, L = 2, I = 3, S = 4, Z = 5, T = 6};
        private readonly Point[][] piece; // [rotation id][block number][0 = x, 1 = y]
        private int rotation;
        private bool[] appearance;
        
        /// <summary>
        /// A block can have one of three appearances encoded by two bits. The four blocks of a tetromino share the same appearance.
        /// The first bit encodes the block type (A or B) and the second encodes the color (A or B). If both bits are set to 0, there is no block.
        /// </summary>
        public bool[] Appearance
        {
            get { return this.appearance; }
        }

        /// <summary>
        /// Returns the type of the tetromino.
        /// </summary>
        public Type_t Type
        {
            get;
        }

        /// <summary>
        /// Constructor. Creates the matrix containing all the rotations for the given tetromino.
        /// </summary>
        /// <param name="t">Type of the tetromino.</param>
        public Tetromino(Type_t t)
        {
            this.Type = t;
            this.piece = new Point[4][];
            for (int i = 0; i < 4; i++)
            {
                this.piece[i] = new Point[4];
                for (int j = 0; j < 4; j++)
                    this.piece[i][j] = new Point();
            }

            this.piece[0][0].X = this.piece[0][0].Y = this.piece[1][0].X = this.piece[1][0].Y =
                this.piece[2][0].X = this.piece[2][0].Y = this.piece[3][0].X = this.piece[3][0].Y = 0; // The pivot is always at (0,0).

            this.appearance = new bool[2];
            switch (t)
            {
                case Type_t.O: // The four rotations yield the same result.
                    this.piece[0][1].X = this.piece[1][1].X = this.piece[2][1].X = this.piece[3][1].X = 1;
                    this.piece[0][1].Y = this.piece[1][1].Y = this.piece[2][1].Y = this.piece[3][1].Y = 0;
                    this.piece[0][2].X = this.piece[1][2].X = this.piece[2][2].X = this.piece[3][2].X = 0;
                    this.piece[0][2].Y = this.piece[1][2].Y = this.piece[2][2].Y = this.piece[3][2].Y = 1;
                    this.piece[0][3].X = this.piece[1][3].X = this.piece[2][3].X = this.piece[3][3].X = 1;
                    this.piece[0][3].Y = this.piece[1][3].Y = this.piece[2][3].Y = this.piece[3][3].Y = 1;
                    this.appearance[0] = false;
                    this.appearance[1] = true;
                    break;
                case Type_t.I: // Rotations 0 and 2, and rotations 1 and 3 are equal.
                    this.piece[0][1].X = this.piece[2][1].X = 1;
                    this.piece[0][1].Y = this.piece[2][1].Y = 0;
                    this.piece[0][2].X = this.piece[2][2].X = -1;
                    this.piece[0][2].Y = this.piece[2][2].Y = 0;
                    this.piece[0][3].X = this.piece[2][3].X = -2;
                    this.piece[0][3].Y = this.piece[2][3].Y = 0;
                    this.piece[1][1].X = this.piece[3][1].X = 0;
                    this.piece[1][1].Y = this.piece[3][1].Y = 1;
                    this.piece[1][2].X = this.piece[3][2].X = 0;
                    this.piece[1][2].Y = this.piece[3][2].Y = -1;
                    this.piece[1][3].X = this.piece[3][3].X = 0;
                    this.piece[1][3].Y = this.piece[3][3].Y = -2;
                    this.appearance[0] = false;
                    this.appearance[1] = true;
                    break;
                case Type_t.S:
                    this.piece[0][1].X = this.piece[2][1].X = -1;
                    this.piece[0][1].Y = this.piece[2][1].Y = 1;
                    this.piece[0][2].X = this.piece[2][2].X = 1;
                    this.piece[0][2].Y = this.piece[2][2].Y = 0;
                    this.piece[0][3].X = this.piece[2][3].X = 0;
                    this.piece[0][3].Y = this.piece[2][3].Y = 1;
                    this.piece[1][1].X = this.piece[3][1].X = 1;
                    this.piece[1][1].Y = this.piece[3][1].Y = 0;
                    this.piece[1][2].X = this.piece[3][2].X = 1;
                    this.piece[1][2].Y = this.piece[3][2].Y = 1;
                    this.piece[1][3].X = this.piece[3][3].X = 0;
                    this.piece[1][3].Y = this.piece[3][3].Y = -1;
                    this.appearance[0] = true;
                    this.appearance[1] = true;
                    break;
                case Type_t.Z:
                    this.piece[0][1].X = this.piece[2][1].X = 0;
                    this.piece[0][1].Y = this.piece[2][1].Y = 1;
                    this.piece[0][2].X = this.piece[2][2].X = 1;
                    this.piece[0][2].Y = this.piece[2][2].Y = 1;
                    this.piece[0][3].X = this.piece[2][3].X = -1;
                    this.piece[0][3].Y = this.piece[2][3].Y = 0;
                    this.piece[1][1].X = this.piece[3][1].X = -1;
                    this.piece[1][1].Y = this.piece[3][1].Y = 0;
                    this.piece[1][2].X = this.piece[3][2].X = -1;
                    this.piece[1][2].Y = this.piece[3][2].Y = 1;
                    this.piece[1][3].X = this.piece[3][3].X = 0;
                    this.piece[1][3].Y = this.piece[3][3].Y = -1;
                    this.appearance[0] = true;
                    this.appearance[1] = false;
                    break;
                case Type_t.J: // Every rotation is different.
                    this.piece[0][1].X = -1;
                    this.piece[0][1].Y = 0;
                    this.piece[0][2].X = 1;
                    this.piece[0][2].Y = 0;
                    this.piece[0][3].X = 1;
                    this.piece[0][3].Y = 1;
                    this.piece[1][1].X = 0;
                    this.piece[1][1].Y = 1;
                    this.piece[1][2].X = 0;
                    this.piece[1][2].Y = -1;
                    this.piece[1][3].X = 1;
                    this.piece[1][3].Y = -1;
                    this.piece[2][1].X = -1;
                    this.piece[2][1].Y = 0;
                    this.piece[2][2].X = 1;
                    this.piece[2][2].Y = 0;
                    this.piece[2][3].X = -1;
                    this.piece[2][3].Y = -1;
                    this.piece[3][1].X = 0;
                    this.piece[3][1].Y = -1;
                    this.piece[3][2].X = 0;
                    this.piece[3][2].Y = 1;
                    this.piece[3][3].X = -1;
                    this.piece[3][3].Y = 1;
                    this.appearance[0] = true;
                    this.appearance[1] = true;
                    break;
                case Type_t.L:
                    this.piece[0][1].X = -1;
                    this.piece[0][1].Y = 0;
                    this.piece[0][2].X = 1;
                    this.piece[0][2].Y = 0;
                    this.piece[0][3].X = -1;
                    this.piece[0][3].Y = 1;
                    this.piece[1][1].X = 0;
                    this.piece[1][1].Y = -1;
                    this.piece[1][2].X = 0;
                    this.piece[1][2].Y = 1;
                    this.piece[1][3].X = 1;
                    this.piece[1][3].Y = 1;
                    this.piece[2][1].X = -1;
                    this.piece[2][1].Y = 0;
                    this.piece[2][2].X = 1;
                    this.piece[2][2].Y = 0;
                    this.piece[2][3].X = 1;
                    this.piece[2][3].Y = -1;
                    this.piece[3][1].X = 0;
                    this.piece[3][1].Y = 1;
                    this.piece[3][2].X = 0;
                    this.piece[3][2].Y = -1;
                    this.piece[3][3].X = -1;
                    this.piece[3][3].Y = -1;
                    this.appearance[0] = true;
                    this.appearance[1] = false;
                    break;
                case Type_t.T:
                    this.piece[0][1].X = -1;
                    this.piece[0][1].Y = 0;
                    this.piece[0][2].X = 1;
                    this.piece[0][2].Y = 0;
                    this.piece[0][3].X = 0;
                    this.piece[0][3].Y = 1;
                    this.piece[1][1].X = 0;
                    this.piece[1][1].Y = -1;
                    this.piece[1][2].X = 1;
                    this.piece[1][2].Y = 0;
                    this.piece[1][3].X = 0;
                    this.piece[1][3].Y = 1;
                    this.piece[2][1].X = 0;
                    this.piece[2][1].Y = -1;
                    this.piece[2][2].X = 1;
                    this.piece[2][2].Y = 0;
                    this.piece[2][3].X = -1;
                    this.piece[2][3].Y = 0;
                    this.piece[3][1].X = 0;
                    this.piece[3][1].Y = -1;
                    this.piece[3][2].X = 0;
                    this.piece[3][2].Y = 1;
                    this.piece[3][3].X = -1;
                    this.piece[3][3].Y = 0;
                    this.appearance[0] = false;
                    this.appearance[1] = true;
                    break;
            }

            this.rotation = 0;
        }
        /// <summary>
        /// Rotates the tetromino counterclockwise.
        /// </summary>
        public void RotateLeft()
        {
            this.rotation = (this.rotation + 1) % 4;
        }
        /// <summary>
        /// Rotates the tetromino clockwise.
        /// </summary>
        public void RotateRight()
        {
            this.rotation = (this.rotation + 3) % 4;
        }
        /// <summary>
        /// Gets the current rotation's blocks.
        /// </summary>
        /// <returns>The four blocks for the current rotation.</returns>
        public Point[] Get()
        {
            return this.piece[this.rotation];
        }
    }
}
