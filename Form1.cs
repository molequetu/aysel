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
       
        Game game;
        
        
        // base hero
        //Character aysel;
        // quest giver
        Character ilhan;
        Dialogue ilhanDialogue;

        Character ants;
        Character farmers;

        //int ticks;
        int drawLast = 0;
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

            // create game 
            createGame();
            // create the map
            createMap();
            
            // create player Aysel
            createAysel();
            // spawn Aysel on entry
            spawnAyselOnTileEntry();
           
            // load ilhan, the quest manager
            createIlhan();


            // create quest dialogue
            ilhanDialogue = new Dialogue(ref game);

            while (!game.GameOver)
            {
                doUpdate();
            }
            Application.Exit();
        }

        private void createGame()
        {
            Form form = (Form)this;
            game = new Game(ref form, 800, 600);
            game.SetFont("Arial", 14, FontStyle.Regular);

        }

        private void createMap()
        {
            game.map = new Level(ref game, 25, 19, 32);

            if (!game.map.loadPalette("palette.bmp", 5))
            {
                MessageBox.Show("Error loading pallet");
                Application.Exit();
            }
            if(!game.map.loadTilemap("map1.level"))
            {
                MessageBox.Show("Error loading level");
                Application.Exit();
            }

            game.map.GridPos = new Point(0, 0);
            game.map.Update();

        }


        private void createAysel()
        {
            game.aysel = new Player(ref game);
            if (!game.aysel.Load("aysel.char"))
            {
                MessageBox.Show("Error loading aysel.char");
                Application.Exit();
            }
            game.aysel.AnimationState = Character.AnimationStates.Standing;
            game.aysel.Position = new Point(0, 0);

            //aysel.Position = new Point(400 - 48, 300 - 48);
            //aysel.Position = new Point(200, 450);
            //aysel.Position = new Point(40, 428);
        }
        private void spawnAyselOnTileEntry()
        {

            Point target = new Point(0, 0);
            bool found = false;
            for (int y = 0; y < 128; y++)
            {
                if (found) break;
                for (int x = 0; x < 128; x++)
                {
                    target = new Point(x - 1, y - 1);
                    Level.tilemapStruct tile = game.map.getTile(x, y);
                    if (tile.data1.ToUpper() == "ENTRY")
                    {
                        found = true;
                        game.aysel.Position = new Point(
                            target.X * 32, target.Y * 32 - 16);
                        break;
                    }
                }
            }
        }
        private void createIlhan()
        {
            ilhan = new Character(ref game);
            if(!ilhan.Load("ilhan.char"))
            {
                MessageBox.Show("Error loading ilhan.char");
                Application.Exit();
                throw new Exception("ilhan.char filename does not exist");
            }
            ilhan.Position = new Point(352, 164);
        }


    
       private void updateHeroDir()
       {
           if (game.keyState.up && game.keyState.right)
                game.aysel.Direction = 1;
           else if (game.keyState.right && game.keyState.down)
               game.aysel.Direction = 3;
           else if (game.keyState.down && game.keyState.left)
               game.aysel.Direction = 5;
           else if (game.keyState.left && game.keyState.up)
                game.aysel.Direction = 7;
           else if (game.keyState.up) game.aysel.Direction = 0;
           else if (game.keyState.right) game.aysel.Direction = 2;
           else if (game.keyState.down) game.aysel.Direction = 4;
           else if (game.keyState.left) game.aysel.Direction = 6;
           else game.aysel.Direction = -1;

       }
        private void doAysel()
        {
            //limit player sprite to the screen boundary
            if (game.aysel.X < -32) game.aysel.X = -32;
            else if (game.aysel.X > 800 - 65) game.aysel.X = 800 - 65;
            if (game.aysel.Y < -48) game.aysel.Y = -48;
            else if (game.aysel.Y > 600 - 81) game.aysel.Y = 600 - 81;

            // orient aysel to the right direction  and draw
            updateHeroDir();
            game.aysel.Draw();

        }
        /*
         * Draw the quests manager sprite
         * in order of Aysel's scroll position
         */
        private void doIlhan()
        {
            float relativeX = 0, relativeY = 0;
            int talkRadius = 70;
            Pen color;

            //draw the vendor sprite
            if (ilhan.X > game.map.ScrollPos.X &&
                ilhan.X < game.map.ScrollPos.X + 23 * 32 &&
                ilhan.Y > game.map.ScrollPos.Y &&
                ilhan.Y < game.map.ScrollPos.Y + 17 * 32)
            {
                relativeX = Math.Abs(game.map.ScrollPos.X - ilhan.X);
                relativeY = Math.Abs(game.map.ScrollPos.Y - ilhan.Y);
                ilhan.GetSprite.Draw((int)relativeX, (int)relativeY);
            }

            //get center of hero sprite
            PointF heroCenter = game.aysel.FootPos;
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
                else  talking = false; 
            
            System.Diagnostics.Debug.Write("talking" +talking);
            System.Diagnostics.Debug.Write("talkflag" + talkFlag);
        }
        
        private void doDialogue()
        {
            if (!talking) return;

            //prepare the dialogue
            ilhanDialogue.Title = "Ilhan The LifeLover";
            ilhanDialogue.Message = "Greetings visitor. Oh my goodness, you look " +
                "like you've  just come to Avgo, and you are ready to fight against the haters." +
                "Your sister, Anna, is prisoned inside a small forest. You will notice" +
                "that the are some spiders arround when searching her. Be carefull, the are not harmfull yet but" +
                "they will soon be. The haters are the farmers..they are sowing ant seeds. ! "+
                "Be sure you pick up all the available gold in the island, becouse you will need"+
                "it to fight the dragon...Good luck Aysel finding your sister.";
            ilhanDialogue.setButtonText(1, "Thank you, Ilhan");
          
            //reposition dialogue window 
            if (game.aysel.CenterPos.X < 400)
            {
                if (game.aysel.CenterPos.Y < 300)
                    ilhanDialogue.setCorner(Dialogue.Positions.LowerRight);
                else
                    ilhanDialogue.setCorner(Dialogue.Positions.UpperRight);
            } else {
                if (game.aysel.CenterPos.Y < 300)
                    ilhanDialogue.setCorner(Dialogue.Positions.LowerLeft);
                else
                    ilhanDialogue.setCorner(Dialogue.Positions.UpperLeft);
            }

            //draw dialogue and look for selection
            ilhanDialogue.updateMouse(game.MousePos, game.MouseButton);
            ilhanDialogue.Draw();
            if (ilhanDialogue.Selection > 0)
            {
                talking = false;
                ilhanDialogue.Selection = 0;
            }
    
       }

   

        private void doUpdate()
        {
            //move the tilemap scroll position
            int steps = 4;

            // take maps scroll position
            PointF pos = game.map.ScrollPos;

            //up key movement
            if (game.keyState.up)
            {
                if (game.aysel.Y > 300 - 48) game.aysel.Y -= steps;
                else
                {
                    pos.Y -= steps;
                    if (pos.Y <= 0) game.aysel.Y -= steps;
                }

            }
            //down key movement
            else if (game.keyState.down)
            {
                if (game.aysel.Y < 300 - 48)
                    game.aysel.Y += steps;
                else
                {
                    pos.Y += steps;
                    if (pos.Y >= (127 - 19) * 32) game.aysel.Y += steps;
                }
            }

            //left key movement
            if (game.keyState.left)
            {
                if (game.aysel.X > 400 - 48) game.aysel.X -= steps;
                else
                {
                    pos.X -= steps;
                    if (pos.X <= 0) game.aysel.X -= steps;
                }
            }
            //right key movement
            else if (game.keyState.right)
            {
                if (game.aysel.X < 400 - 48) game.aysel.X += steps;
                else
                {
                    pos.X += steps;
                    if (pos.X >= (127 - 25) * 32) game.aysel.X += steps;
                }
            }

            // set posisition to level scroll position and then update level
           // if (pos.X < 0) pos.X = 0;
            //if (pos.Y < 0) pos.Y = 0;
            game.map.ScrollPos = pos;
            game.map.Update();

           

            // get the untimed core frame rate
            int frameRate = game.FrameRate();

            //drawing code should be limited to 60 fps
            int ticks = Environment.TickCount;
            if (ticks > drawLast + 16)
            {
                drawLast = ticks;
                // draw the map
                game.map.Draw(0, 0, 800, 600);
                
                // draw and animate aysel
                doAysel();
                // draw the quest manager
                doIlhan();
                // make ilhan and aysel talk
                doDialogue();

                // print text to form
                //print stats
                game.Print(700, 0, frameRate.ToString());
                int y = 0;
                game.Print(0, y, "Scroll " + game.map.ScrollPos.ToString());
                y += 20;
                game.Print(0, y, "Player " + game.aysel.Position.ToString());
                y += 20;


                Point feet = HeroFeet();

                int tilex = (int)(game.map.ScrollPos.X + feet.X) / 32;
                int tiley = (int)(game.map.ScrollPos.Y + feet.Y) / 32;
                Level.tilemapStruct ts = game.map.getTile(tilex, tiley);
                game.Print(0, y, "Tile " + tilex.ToString() + "," +
                    tiley.ToString() + " = " + ts.tilenum.ToString());
                y += 20;
                if (ts.collidable)
                {
                    game.Print(0, y, "Collidable");
                    y += 20;
                }
            

                //highlight collision areas around player
                game.Device.DrawRectangle(Pens.Blue, game.aysel.GetSprite.Bounds);
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
            return new Point((int)(game.aysel.X + 32), (int)(game.aysel.Y + 32 + 16));
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
                case Keys.W: game.keyState.up = true; break;
                case Keys.Down:
                case Keys.S: game.keyState.down = true; break;
                case Keys.Left:
                case Keys.A: game.keyState.left = true; break;
                case Keys.Right:
                case Keys.D: game.keyState.right = true; break;
                case Keys.Space: talkFlag = true; break;

            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape: game.GameOver = true; break;
                case Keys.Up:
                case Keys.W: game.keyState.up = false; break;
                case Keys.Down:
                case Keys.S: game.keyState.down = false; break;
                case Keys.Left:
                case Keys.A: game.keyState.left = false; break;
                case Keys.Right:
                case Keys.D: game.keyState.right = false; break;
                case Keys.Space: talkFlag = false; break;
                case Keys.D1:
                    game.aysel.AnimationState = Character.AnimationStates.Walking;
                    break;
                case Keys.D2:
                    game.aysel.AnimationState = Character.AnimationStates.Attacking;
                    break;
                case Keys.D3:
                    game.aysel.AnimationState = Character.AnimationStates.Dying;
                    break;
                
            }
        }

        private void Form1_FormClosed_1(object sender, FormClosedEventArgs e)
        {
            game.GameOver = true;
        }

  }
}