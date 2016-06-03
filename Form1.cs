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
        
        // base hero
        Character aysel;
        // quest giver
        Character ilhan;


        int ticks;
        int drawLast = 0;

        bool portalFlag = false;
        Point portalTarget;

        // quest manager helper's
        bool talkFlag = false;
        bool talking = false;

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
            //load  aysel
            createAysel();
            // load ilhan, the quest manager
            createIlhan();

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

        private void createAysel()
        {
            aysel = new Character(ref game);
            aysel.Load("aysel.char");
            //aysel.Position = new Point(400 - 48, 300 - 48);
            aysel.Position = new Point(180, 428);
        }

        private void createIlhan()
        {
            ilhan = new Character(ref game);
            ilhan.Load("ilhan.char");
            ilhan.Position = new Point(352, 164);
        }

        
        
        /*
       private void createHero()
       {
           aysel = new Sprite(ref game);
           aysel.Image = game.LoadBitmap("aysel_walk.png");
           aysel.Columns = 8;
           aysel.TotalFrames = 8 * 8;
           aysel.Size = new Size(128, 128);
           aysel.Position = new PointF(50, 490);
           aysel.CurrentFrame = game.Random(64);
       }
       */
       private void updateHeroDir()
       {
           if (keyState.up && keyState.right)
               aysel.Direction = 1;
           else if (keyState.right && keyState.down)
               aysel.Direction = 3;
           else if (keyState.down && keyState.left)
               aysel.Direction = 5;
           else if (keyState.left && keyState.up)
               aysel.Direction = 7;
           else if (keyState.up) aysel.Direction = 0;
           else if (keyState.right) aysel.Direction = 2;
           else if (keyState.down) aysel.Direction = 4;
           else if (keyState.left) aysel.Direction = 6;
           else aysel.Direction = -1;

       }
        private void doAysel()
        {
            //limit player sprite to the screen boundary
            if (aysel.X < -32) aysel.X = -32;
            else if (aysel.X > 800 - 65) aysel.X = 800 - 65;
            if (aysel.Y < -48) aysel.Y = -48;
            else if (aysel.Y > 600 - 81) aysel.Y = 600 - 81;

            // orient aysel to the right direction  and draw
            updateHeroDir();
            aysel.Draw();

        }
        private void doIlhan()
        {
            float relativeX = 0, relativeY = 0;
            int talkRadius = 70;
            Pen color;

            //draw the vendor sprite
            if (ilhan.X > map.ScrollPos.X &&
                ilhan.X < map.ScrollPos.X + 23 * 32 &&
                ilhan.Y > map.ScrollPos.Y &&
                ilhan.Y < map.ScrollPos.Y + 17 * 32)
            {
                relativeX = Math.Abs(map.ScrollPos.X - ilhan.X);
                relativeY = Math.Abs(map.ScrollPos.Y - ilhan.Y);
                ilhan.GetSprite.Draw((int)relativeX, (int)relativeY);
            }

            //get center of hero sprite
            PointF heroCenter = aysel.FootPos;
            heroCenter.X += 16;
            heroCenter.Y += 16;
            game.Device.DrawRectangle(Pens.Red, heroCenter.X - 2,
                heroCenter.Y - 2, 4, 4);

            //get center of NPC
            PointF questManagerCenter = new Point((int)relativeX, (int)relativeY);
            questManagerCenter.X += ilhan.GetSprite.Width / 2;
            questManagerCenter.Y += ilhan.GetSprite.Height / 2;
            game.Device.DrawRectangle(Pens.Red, questManagerCenter.X - 2,
                questManagerCenter.Y - 2, 4, 4);

            double dist = game.Distance(heroCenter, questManagerCenter);

            // if hero is close to NPC draw a line and then activate-like diaologue
            if (dist < 270)
            {
                if (dist < talkRadius)
                    color = new Pen(Brushes.Blue, 2.0f);
                else
                    color = new Pen(Brushes.Red, 2.0f);
                game.Device.DrawLine(color, heroCenter, questManagerCenter);

                //draw circle around vendor to show talk radius
                float spriteSize = ilhan.GetSprite.Width / 2;
                float centerx = relativeX + spriteSize;
                float centery = relativeY + spriteSize;
                RectangleF circleRect = new RectangleF(centerx - talkRadius,
                    centery - talkRadius, talkRadius * 2, talkRadius * 2);
                game.Device.DrawEllipse(color, circleRect);

            }

            //is playing trying to talk to this quest manager?
            if (dist < talkRadius)
            {
                if (talkFlag) talking = true;
            }
            else talking = false;
        }

        private void doUpdate()
        {
            //move the tilemap scroll position
            int steps = 4;
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
            if (pos.X < 0) pos.X = 0;
            if (pos.Y < 0) pos.Y = 0;
            map.ScrollPos = pos;
            map.Update();

           

            // get the untimed core frame rate
            int frameRate = game.FrameRate();

            //drawing code should be limited to 60 fps
            ticks = Environment.TickCount;
            if (ticks > drawLast + 16)
            {
                drawLast = ticks;
                // draw the map
                map.Draw(0, 0, 800, 600);
                
                // draw and animate aysel
                doAysel();
                // draw the quest manager
                doIlhan();


                // print text to form
                //print stats
                game.Print(700, 0, frameRate.ToString());
                int y = 0;
                game.Print(0, y, "Scroll " + map.ScrollPos.ToString());
                y += 20;
                game.Print(0, y, "Player " + aysel.Position.ToString());
                y += 20;


                Point feet = HeroFeet();

                int tilex = (int)(map.ScrollPos.X + feet.X) / 32;
                int tiley = (int)(map.ScrollPos.Y + feet.Y) / 32;
                Level.tilemapStruct ts = map.getTile(tilex, tiley);
                game.Print(0, y, "Tile " + tilex.ToString() + "," +
                    tiley.ToString() + " = " + ts.tilenum.ToString());
                y += 20;
                if (ts.collidable)
                {
                    game.Print(0, y, "Collidable");
                    y += 20;
                }
                if (ts.portal)
                {
                    game.Print(0, y, "Portal to " + ts.portalx.ToString() +
                        "," + ts.portaly.ToString());
                    portalFlag = true;
                    portalTarget = new Point(ts.portalx - feet.X / 32,
                        ts.portaly - feet.Y / 32);
                    y += 20;
                }
                else
                    portalFlag = false;

                //highlight collision areas around player
                game.Device.DrawRectangle(Pens.Blue, aysel.GetSprite.Bounds);
                game.Device.DrawRectangle(Pens.Red, feet.X + 16 - 1,
                    feet.Y + 16 - 1, 2, 2);
                game.Device.DrawRectangle(Pens.Red, feet.X, feet.Y, 32, 32);

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
        private Point HeroFeet()
        {
            return new Point((int)(aysel.X + 32), (int)(aysel.Y + 32 + 16));
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
                case Keys.Space:
                    if (portalFlag) map.GridPos = portalTarget;
                    break;
                case Keys.D1:
                    aysel.AnimationState = Character.AnimationStates.Walking;
                    break;
                case Keys.D2:
                    aysel.AnimationState = Character.AnimationStates.Attacking;
                    break;
                case Keys.D3:
                    aysel.AnimationState = Character.AnimationStates.Dying;
                    break;

            }
        }

        private void Form1_FormClosed_1(object sender, FormClosedEventArgs e)
        {
            game.GameOver = true;
        }

  }
}