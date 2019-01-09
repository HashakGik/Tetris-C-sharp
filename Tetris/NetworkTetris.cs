using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Tetris
{
    /// <summary>
    /// Class derived from Tetris which allows to send and receive state updates via a network stream. A worker thread interacts with the stream.
    /// Since it interacts with an already opened stream, it can work with any protocol.
    /// </summary>
    class NetworkTetris : Tetris
    {
        private bool done;
        private bool notifiedGameOver;
        private bool gameover;
        private byte[] sendBuffer, receiveBuffer;
        private List<BitArray> image;
        private NetworkStream stream;
        /// <summary>
        /// Termination condition for the worker thread.
        /// </summary>
        private bool Done
        {
            get
            {
                lock(this) // lock() works only on references, a bool is always a value, therefore it's can't be used inside a lock instruction.
                {
                    return this.done;
                }
            }
            set
            {
                lock (this)
                {
                    this.done = value;
                }
            }
        }
        private Thread thd;
        /// <summary>
        /// Connection error delegate.
        /// </summary>
        public GameOver OnConnectionError { get; set; }
        /// <summary>
        /// True if this instance is a transmitter.
        /// </summary>
        public bool Sender { get; set; }
        /// <summary>
        /// True if the player hosts the game.
        /// </summary>
        public bool Host { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="w">Width of the playing field.</param>
        /// <param name="h">Height of the playing field.</param>
        /// <param name="stream">Network stream.</param>
        /// <param name="sender">True if this instance sends commands over the stream, false if this instance receives commands from the stream.</param>
        /// <param name="host">True if the player hosts the game.</param>
        /// <param name="level">Initial level.</param>
        /// <param name="bMode">True if in mode B.</param>
        /// <param name="bHeight">Initial height for mode B.</param>
        public NetworkTetris(int w, int h, NetworkStream stream, bool sender, bool host, uint level = 1, bool bMode = false, uint bHeight = 0) :
            base(w, h, level, bMode, bHeight)
        {
            this.Sender = sender;
            this.sendBuffer = new byte[12 * 4 + 2 * this.w * this.h / 8 + 1];
            this.receiveBuffer = new byte[12 * 4 + 2 * this.w * this.h / 8 + 1];

            for (int i = 0; i < this.sendBuffer.Length; i++)
                this.sendBuffer[i] = this.receiveBuffer[i] = 0x00;

            this.image = new List<BitArray>(this.h);
            for (int i = 0; i < this.h; i++)
                this.image.Add(new BitArray(this.w * 2));
                


            this.stream = stream;
            this.stream.ReadTimeout = 5000;
            this.Host = host;

            this.gameover = false;
            this.notifiedGameOver = false;

            try
            {
                
                if (this.Host && this.Sender) // If the player is the host, send the initial level, the initial height and the game mode to the opponent.
                {
                    this.sendBuffer[0] = BitConverter.GetBytes(this.Level)[0];
                    this.sendBuffer[1] = BitConverter.GetBytes(this.Level)[1];
                    this.sendBuffer[2] = BitConverter.GetBytes(this.Level)[2];
                    this.sendBuffer[3] = BitConverter.GetBytes(this.Level)[3];
                    this.sendBuffer[4] = BitConverter.GetBytes(this.bHeight)[0];
                    this.sendBuffer[5] = BitConverter.GetBytes(this.bHeight)[1];
                    this.sendBuffer[6] = BitConverter.GetBytes(this.bHeight)[2];
                    this.sendBuffer[7] = BitConverter.GetBytes((this.bMode)? 1 : 0)[0];
                    this.stream.Write(this.sendBuffer, 0, 8);
                }
                else if (!this.Host && this.Sender) // If the player is not the host, it must receive the true initial parameters from the host.
                {
                    this.stream.Read(this.receiveBuffer, 0, 4);
                    this.Level = BitConverter.ToUInt32(this.receiveBuffer, 0);
                    this.stream.Read(this.receiveBuffer, 0, 4);
                    if (this.receiveBuffer[3] == 1)
                    {
                        this.receiveBuffer[3] = 0;
                        this.bHeight = BitConverter.ToUInt32(this.receiveBuffer, 0);
                        this.bMode = true;

                        this.Lines = 25;
                        int numBlocks = (int)(this.w * h * this.bHeight * (this.r.NextDouble() + 6) / 100.0);
                        int tmp;

                        for (int i = 0, k = 0; i < this.h; i++)
                            for (int j = 0; j < this.w; j++)
                                if (k < numBlocks && this.r.Next(2) == 1)
                                {
                                    tmp = this.r.Next(3) + 1;
                                    this.field[i][2 * j] = tmp % 2 != 0;
                                    this.field[i][2 * j + 1] = tmp / 2 != 0;

                                    k++;
                                }
                    }
                    else
                    {
                        this.bHeight = 0;
                        this.bMode = false;
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnConnectionError();
            }


            // On game over and victory it must stop receiving local inputs, but it must still send the state to the opponent, in order to avoid a timeout.
            this.OnGameOver += delegate ()
            {
                if (this.Sender)
                {
                    this.gameover = true;
                }
            };
            this.OnVictory += delegate ()
            {
                if (this.Sender)
                {
                    this.gameover = true;
                }
            };

            this.OnConnectionError += delegate ()
            {
                this.Done = true;
            };


            if (!this.Sender)
            {
                // Creates the worker thread.
                this.thd = new Thread(() => { while (!this.Done) this.Update(); });
                this.thd.Start();
            }
        }
        /// <summary>
        /// Stops the worker thread.
        /// </summary>
        public void Stop()
        {
            this.Done = true;
            this.thd.Join();
        }
        /// <summary>
        /// Updates the game's state. If it's a sender sends the current state over the stream, otherwise it retrieves the current state from the stream.
        /// </summary>
        public override void Update()
        {
            if (this.Sender)
            {
                base.Update();
                this.Serialize();
            }
            else
                this.Deserialize();
        }
        /// <summary>
        /// Moves the tetromino down and sends the current state over the stream. This method should be invoked only on a sender.
        /// </summary>
        public override void MoveDown()
        {
            base.MoveDown();
            this.Serialize();
        }
        /// <summary>
        /// Moves the tetromino left and sends the current state over the stream. This method should be invoked only on a sender.
        /// </summary>
        public override void MoveLeft()
        {
            base.MoveLeft();
            this.Serialize();
        }
        /// <summary>
        /// Moves the tetromino right and sends the current state over the stream. This method should be invoked only on a sender.
        /// </summary>
        public override void MoveRight()
        {
            base.MoveRight();
            this.Serialize();
        }
        /// <summary>
        /// Rotates the tetromino counterclockwise and sends the current state over the stream. This method should be invoked only on a sender.
        /// </summary>
        public override void RotateLeft()
        {
            base.RotateLeft();
            this.Serialize();
        }
        /// <summary>
        /// Rotates the tetromino clockwise and sends the current state over the stream. This method should be invoked only on a sender.
        /// </summary>
        public override void RotateRight()
        {
            base.RotateRight();
            this.Serialize();
        }
        /// <summary>
        /// Encodes the current state and sends it over the network stream. Each "packet" has a fixed size of 48 bytes + width * height * 2 bits.
        /// The first 48 bytes are treated as 12 Int32 which encode the following data:
        /// - 0: Running game if 0, game over if 1. the remaining 31 bits can be used to encode more flags in the future.
        /// - 1..7: Tetrominoes' statistics
        /// - 8..9: Current score (Int64)
        /// - 10: Current level
        /// - 11: Lines cleared
        /// The remaining bits encode the playing field.
        /// </summary>
        public void Serialize()
        {
            byte[] tmp32 = new byte[4];
            byte[] tmp64 = new byte[8];

            try
            {
                for (int i = 0; i < this.sendBuffer.Length; i++)
                    this.sendBuffer[i] = 0;

                tmp32 = BitConverter.GetBytes((this.gameover)? 1: 0); // Game over.
                tmp32.CopyTo(this.sendBuffer, 0);
                for (int i = 0; i < 7; i++)
                {
                    tmp32 = BitConverter.GetBytes(this.Statistics[i]); // Statistics.
                    tmp32.CopyTo(this.sendBuffer, 4 * (i + 1));
                }
                tmp64 = BitConverter.GetBytes(this.Score); // Current score.
                tmp64.CopyTo(this.sendBuffer, 4 * 8);

                tmp32 = BitConverter.GetBytes(this.Level); // Current level.
                tmp32.CopyTo(this.sendBuffer, 4 * 10);
                tmp32 = BitConverter.GetBytes(this.Lines); // Lines cleared.
                tmp32.CopyTo(this.sendBuffer, 4 * 11);

                // Playing field.
                this.image = base.GetImage();
                for (int i = 0; i < this.h; i++)
                    for (int j = 0; j < 2 * this.w; j++)
                        if (this.image[i][j])
                            this.sendBuffer[4 * 12 + (2 * i * this.w + j) / 8] |= (byte)(1 << (2 * i * this.w + j) % 8);

                this.stream.Write(this.sendBuffer, 0, this.sendBuffer.Length);
            }
            catch (Exception ex)
            {
                if (this.OnConnectionError != null)
                    this.OnConnectionError();
            }
        }
        /// <summary>
        /// Retrieves a "packet" from the network stream and updates the local state accordingly.
        /// </summary>
        private void Deserialize()
        {
            try
            {
                this.stream.Read(this.receiveBuffer, 0, this.receiveBuffer.Length); // BUG: deadlock at the end of the game.

                if (BitConverter.ToInt32(this.receiveBuffer, 0) != 0 && !this.notifiedGameOver)
                {
                    //this.Done = true;
                    this.OnGameOver();
                    this.notifiedGameOver = true; // In order to avoid an endless loop, it notifies the game over only once.
                }
                    

                for (int i = 0; i < 7; i++)
                    this.Statistics[i] = BitConverter.ToInt32(this.receiveBuffer, 4 * (i + 1));
                this.Score = BitConverter.ToInt64(this.receiveBuffer, 4 * 8);
                this.Level = BitConverter.ToUInt32(this.receiveBuffer, 4 * 10);
                this.Lines = BitConverter.ToInt32(this.receiveBuffer, 4 * 11);

                lock(this.image)
                {
                    for (int i = 0; i < this.h; i++)
                        for (int j = 0; j < 2 * this.w; j++)
                        {
                            // Converts this.receiveBuffer[4 * 12 ... Length] in this.image[i][j]
                            this.image[i][j] = ((this.receiveBuffer[4 * 12 + (2 * i * this.w + j) / 8] & (byte)(1 << (2 * i * this.w + j) % 8)) != 0);
                        }
                }
            }
            catch (Exception ex)
            {
                if (this.OnConnectionError != null)
                    this.OnConnectionError();
            }
        }
        /// <summary>
        /// Gets the current playing field. If it's a receiver  it must synchronize with the worker thread.
        /// </summary>
        /// <returns>Playing field (see Tetris class).</returns>
        public override List<BitArray> GetImage()
        {
            List<BitArray> ret;

            
            if (this.Sender)
                ret = base.GetImage();
            else
                lock (this.image)
                {
                    ret = this.image;
                }

            return ret;
        }
    }
}
