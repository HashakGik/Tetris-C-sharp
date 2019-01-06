using System;
using System.Windows.Forms;

namespace Tetris
{
    /// <summary>
    /// This program is a Tetris game (Nintendo Entertainment System rule set and look & feel, except for the long levels and the color palette bugs).
    /// It allows both single (local) and multi player (over TCP) games in A (endurance) and B (clear 25 lines to win) modes.
    /// Every string is defined as a resource, so it's easily localisable.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StartForm());
        }
    }
}
