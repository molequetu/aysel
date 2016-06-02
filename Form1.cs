using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;


namespace Aysel
{
    public partial class Form1 : Form
    {   
        public struct keyStates
        {
            public bool up, down, left, right;
        }

        Game game;
        Level map;
        keyStates keyState;
        Sprite aysel;
        int ayselDir = 0;
        int ticks;
        int drawLast = 0;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Aysel, chronicle I";

            // create game object
            Form form = (Form)this;
            game = new Game(ref form, 800, 600);

            // create the map
            createMap();
            // create the hero
            createHero();


            while (!game.GameOver)
            {
                doUpdate();
            }
            Application.Exit();
        }


        private void createMap()
        {
            map = new Level(ref game, 25, 19, 32);
            map.loadPalette("palette.bmp", 5);
            map.loadTilemap("map1.level");
        }

        private void createHero()
        {
            aysel = new Sprite(ref game);
            aysel.Image = game.LoadBitmap("hero_girl_axe_shield_walk.png");
            aysel.Columns = 8;
            aysel.TotalFrames = 8 * 8;
            aysel.Size = new Size(128, 128);
            aysel.Position = new PointF(50, 490);
            aysel.CurrentFrame = game.Random(72);
        }

        private void doUpdate()
        {
            //move the tilemap scroll position
            int steps = 8;
            // take maps scroll position


            PointF pos =  map.ScrollPos;

            //up key movement
            if (keyState.up)
            {
                if (aysel.Y > 300 - 48) aysel.Y -= steps;
                else
                {
                    pos.Y -= steps;
                    if (pos.Y <= 0) aysel.Y -= steps;
                }

            }
            //down key movement
            else if (keyState.down)
            {
                if (aysel.Y < 300 - 48)
                    aysel.Y += steps;
                else
                {
                    pos.Y += steps;
                    if (pos.Y >= (127 - 19) * 32) aysel.Y += steps;
                }
            }

            //left key movement
            if (keyState.left)
            {
                if (aysel.X > 400 - 48) aysel.X -= steps;
                else
                {
                    pos.X -= steps;
                    if (pos.X <= 0) aysel.X -= steps;
                }
            }
            //right key movement
            else if (keyState.right)
            {
                if (aysel.X < 400 - 48) aysel.X += steps;
                else
                {
                    pos.X += steps;
                    if (pos.X >= (127 - 25) * 32) aysel.X += steps;
                }
            }

            // set posisition to level scroll position and then update level
            map.ScrollPos = pos;
            map.Update();

            //limit player sprite to the screen boundary
            if (aysel.X < -32) aysel.X = -32;
            else if (aysel.X > 800 - 65) aysel.X = 800 - 65;
            if (aysel.Y < -48) aysel.Y = -48;
            else if (aysel.Y > 600 - 81) aysel.Y = 600 - 81;



            //drawing code should be limited to 60 fps
            ticks = Environment.TickCount;
            if (ticks > drawLast + 16)
            {
                drawLast = ticks;
                // draw the map
                map.Draw(0, 0, 800, 600);

                aysel.Draw();

                //refresh window
                game.Update();
                Application.DoEvents();
            }
            else
            {
                //throttle the cpu
                Thread.Sleep(1);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            game.GameOver = true;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W: keyState.up = true; break;
                case Keys.Down:
                case Keys.S: keyState.down = true; break;
                case Keys.Left:
                case Keys.A: keyState.left = true; break;
                case Keys.Right:
                case Keys.D: keyState.right = true; break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape: game.GameOver = true; break;
                case Keys.Up:
                case Keys.W: keyState.up = false; break;
                case Keys.Down:
                case Keys.S: keyState.down = false; break;
                case Keys.Left:
                case Keys.A: keyState.left = false; break;
                case Keys.Right:
                case Keys.D: keyState.right = false; break;
            }
        }

        private void Form1_FormClosed_1(object sender, FormClosedEventArgs e)
        {
            game.GameOver = true;
        }
    }
}
