using System.Drawing;

namespace Tetris
{
    /// <summary>
    /// Palette data structure. It follows the NES color convention  (each level has two different colors and every ten levels the palette repeats).
    /// After level 137 a bug in the original NES game alters the palette. This class does NOT reproduce that bug.
    /// </summary>
    class Palette
    {
        private SolidBrush[,] brushes;

        public Palette()
        {
            this.brushes = new SolidBrush[10, 2];

            this.brushes[0, 0] = new SolidBrush(Color.FromArgb(0x00, 0x58, 0xf8));
            this.brushes[0, 1] = new SolidBrush(Color.FromArgb(0x3c, 0xbc, 0xfc));
            this.brushes[1, 0] = new SolidBrush(Color.FromArgb(0x00, 0xa8, 0x00));
            this.brushes[1, 1] = new SolidBrush(Color.FromArgb(0xb8, 0xf8, 0x18));
            this.brushes[2, 0] = new SolidBrush(Color.FromArgb(0xd8, 0x00, 0xcc));
            this.brushes[2, 1] = new SolidBrush(Color.FromArgb(0xf8, 0x78, 0xf8));
            this.brushes[3, 0] = new SolidBrush(Color.FromArgb(0x00, 0x58, 0xf8));
            this.brushes[3, 1] = new SolidBrush(Color.FromArgb(0x58, 0xd8, 0x54));
            this.brushes[4, 0] = new SolidBrush(Color.FromArgb(0xe4, 0x00, 0x58));
            this.brushes[4, 1] = new SolidBrush(Color.FromArgb(0x58, 0xf8, 0x98));
            this.brushes[5, 0] = new SolidBrush(Color.FromArgb(0x58, 0xf8, 0x98));
            this.brushes[5, 1] = new SolidBrush(Color.FromArgb(0x68, 0x88, 0xfc));
            this.brushes[6, 0] = new SolidBrush(Color.FromArgb(0xf8, 0x38, 0x00));
            this.brushes[6, 1] = new SolidBrush(Color.FromArgb(0x7c, 0x7c, 0x7c));
            this.brushes[7, 0] = new SolidBrush(Color.FromArgb(0x68, 0x44, 0xfc));
            this.brushes[7, 1] = new SolidBrush(Color.FromArgb(0xa8, 0x00, 0x20));
            this.brushes[8, 0] = new SolidBrush(Color.FromArgb(0x00, 0x58, 0xf8));
            this.brushes[8, 1] = new SolidBrush(Color.FromArgb(0xf8, 0x38, 0x00));
            this.brushes[9, 0] = new SolidBrush(Color.FromArgb(0xf8, 0x38, 0x00));
            this.brushes[9, 1] = new SolidBrush(Color.FromArgb(0xfc, 0xa0, 0x44));
        }
        /// <summary>
        /// Gets a color from the palette.
        /// </summary>
        /// <param name="level">Current level.</param>
        /// <param name="blockType">Current block type.</param>
        /// <returns>The requested color.</returns>
        public SolidBrush Get(uint level, bool blockType)
        {
            return brushes[level % 10, (blockType) ? 0 : 1];
        }
    }
}
