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
        
        Dialogue dialogue;
        Character[] ants;
        Character[] farmers;

        //int ticks;
        int drawLast = 0;
        // quest manager helper's
        bool talkFlag = false;
        bool talking = false;

        bool lootFlag = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Aysel, chronicle I";

            // create game 
            createGame();

            // create player Aysel
            createAysel();
            
            // create the map
            createMap();
    
            // spawn Aysel on entry
            spawnAyselOnTileEntry();
           
            // load ilhan, the quest manager
            createIlhan();

            // create quest dialogue
            ilhanDialogue = new Dialogue(ref game);


            // load items
            game.Items = new Items();
            if (!game.Items.Load("items.item"))
            {
                MessageBox.Show("Error loading items");
                Application.Exit();
            }

            /**
            * create inventory
            **/
            game.Inven = new Inventory(ref game, new Point((800 - 532) / 2, 50));
            if (!game.Inven.AddItem(game.Items.getItem("Short Sword")))
            {
                MessageBox.Show("Error adding item to inventory");
                Application.Exit();
            }


            /**
             * create treasure drop list
            **/
            game.Treasure = new List<Game.DrawableItem>();
            lootFlag = false;

            //add one item of treasure to the dungeon
            Item item = game.Items.getItem("Small Shield");
            DropTreasureItem(ref item, 10 * 32, 1 * 32);

            //add loot treasure from a .char file
            Character loot = new Character(ref game);
            if (!loot.Load("dummydrop1.char"))
            {
                MessageBox.Show("Error loading loot file");
                Application.Exit();
            }
            loot.Position = new Point(8 * 32, 2 * 32);
            DropLoot(ref loot);

            //search dungeon level for drop item codes
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    Item it = null;
                    Level.tilemapStruct tile = game.map.getTile(x, y);
                    if (tile.data1.ToUpper() == "ITEM" && tile.data2 != "")
                    {
                        it = game.Items.getItem(tile.data2);
                        DropTreasureItem(ref it, x * 32, y * 32);
                    }
                }
            }
            /**
            * create farmer list
            **/
            farmers = new Character[10];

            //search dungeon level for farmers!
            int count = 0;
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    Level.tilemapStruct tile = game.map.getTile(x, y);
                    if (tile.data1.ToUpper() == "FARMER" && tile.data2 != "")
                    {
                        farmers[count] = new Character(ref game);
                        farmers[count].Load(tile.data2);
                        farmers[count].Position = new PointF((x - 1) * 32, (y - 1) * 32);
                        count++;
                    }
                }
            }
            dialogue = new Dialogue(ref game);

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

            game.map.GridPos = new Point(0, 10);
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
            game.aysel.AnimationState = Character.AnimationStates.Walking;
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
            
            //System.Diagnostics.Debug.Write("talking" +talking);
           // System.Diagnostics.Debug.Write("talkflag" + talkFlag);
        }
        
        private void doIlhanDialogue()
        {
            if (!talking) return;
            //System.Diagnostics.Debug.Write("talking fkag = " + talking);
            
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
        
                if (game.aysel.CenterPos.Y < 270)
                    ilhanDialogue.setCorner(Dialogue.Positions.LowerRight);
                else
                   ilhanDialogue.setCorner(Dialogue.Positions.UpperRight);
            

            //draw dialogue and look for selection
            ilhanDialogue.updateMouse(game.MousePos, game.MouseButton);
            ilhanDialogue.Draw();
            
            if (ilhanDialogue.Selection > 0)
            {
                talkFlag = false;
                ilhanDialogue.Visible = false;
                ilhanDialogue.Selection = 0;
            }
    
       }


        private void doUpdate()
        {
      
            // get the untimed core frame rate
            int frameRate = game.FrameRate();

            //drawing code should be limited to 60 fps
            int ticks = Environment.TickCount;
            if (ticks > drawLast + 16)
            {
                drawLast = ticks;
                // draw the map
                //game.map.Draw(0, 0, 800, 600);
                doScrolling();
                // draw and animate aysel
                doTreasure();
                doAysel();
                // draw the quest manager
                doIlhan();
                // make ilhan and aysel talk
                doIlhanDialogue();
               
                doMonsters();
                doDialogue();
                doInventory();



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
            else Thread.Sleep(1);
        }

        private void doScrolling()
        {
            game.map.Update();
            game.map.Draw(0, 0, 800, 600);
        }
        private void doTreasure()
        {
            PointF relativePos = new PointF(0, 0);
            const int lootRadius = 40;
            PointF heroCenter = game.aysel.CenterPos;
            PointF itemCenter = new PointF(0, 0);
            double dist;

            foreach (Game.DrawableItem it in game.Treasure)
            {
                //is item in view?
                if (it.sprite.X > game.map.ScrollPos.X - 64
                    && it.sprite.X < game.map.ScrollPos.X + 23 * 32 + 64
                    && it.sprite.Y > game.map.ScrollPos.Y - 64
                    && it.sprite.Y < game.map.ScrollPos.Y + 17 * 32 + 64)
                {
                    //get relative position of item on screen
                    relativePos.X = it.sprite.X - game.map.ScrollPos.X;
                    relativePos.Y = it.sprite.Y - game.map.ScrollPos.Y;

                    //get center of item
                    itemCenter = relativePos;
                    itemCenter.X += it.sprite.Width / 2;
                    itemCenter.Y += it.sprite.Height / 2;

                    //get distance to the item
                    dist = game.aysel.CenterDistance(itemCenter);

                    //is player trying to pick up this loot?
                    if (dist < lootRadius)
                    {
                        game.Device.DrawEllipse(new Pen(Color.Magenta, 2),
                            itemCenter.X - it.sprite.Width / 2,
                            itemCenter.Y - it.sprite.Height / 2,
                            it.sprite.Width, it.sprite.Height);

                        if (lootFlag)
                        {
                            //collect gold or item
                            if (it.item.Name == "gold" && it.item.Value > 0)
                            {
                                game.aysel.Gold += (int)it.item.Value;
                                game.Treasure.Remove(it);
                                showDialogue("LOOT", it.item.Value.ToString() + " GOLD", "OK");
                            }
                            else
                            {
                                if (game.Inven.AddItem(it.item))
                                {
                                    game.Treasure.Remove(it);
                                    showDialogue("LOOT", it.item.Summary, "OK");
                                }
                                else
                                    showDialogue("OVERLOADED!", "You are overloaded with too much stuff!", "OK");
                            }

                            //wait for user 
                            if (dialogue.Selection == 1)
                            {
                                lootFlag = false;
                                dialogue.Selection = 0;
                            }
                            break;
                        }
                    }

                    //draw the monster sprite
                    it.sprite.Draw((int)relativePos.X, (int)relativePos.Y);
                }
            }
        }

        private void showDialogue(string title, string message,
        string button1)
        {
            dialogue.Title = title;
            dialogue.Message = message;
            dialogue.NumButtons = 1;
            dialogue.setButtonText(1, button1);
            dialogue.Visible = true;
        }

        private void showDialogue(string title, string message,
            string button1, string button2)
        {
            dialogue.Title = title;
            dialogue.Message = message;
            dialogue.NumButtons = 2;
            dialogue.setButtonText(1, button1);
            dialogue.setButtonText(2, button2);
            dialogue.Visible = true;
        }

        private void doDialogue()
        {
            if (game.aysel.CenterPos.Y < 300)
                dialogue.setCorner(Dialogue.Positions.LowerRight);
            else
                dialogue.setCorner(Dialogue.Positions.UpperRight);

            dialogue.updateMouse(game.MousePos, game.MouseButton);
            dialogue.Draw();

            if (dialogue.Selection > 0)
            {
                dialogue.Visible = false;
                dialogue.Selection = 0;
            }
        }

        private void doInventory()
        {
            if (!game.Inven.Visible) return;
            game.Inven.updateMouse(game.MousePos, game.MouseButton);
            game.Inven.Draw();
        }
        private Point HeroFeet()
        {
            return new Point((int)(game.aysel.X + 32), (int)(game.aysel.Y + 32 + 16));
        }

        //drop loot specified in the monster's character data file
        private void DropLoot(ref Character srcMonster)
        {
            int count = 0;
            int rad = 64;

            //any gold to drop?
            Item itm = new Item();
            int gold = game.Random(srcMonster.DropGoldMin, srcMonster.DropGoldMax);
            itm.Name = "gold";
            itm.DropImageFilename = "gold.png";
            itm.InvImageFilename = "gold.png";
            itm.Value = gold;
            Point p = new Point(0, 10);
            p.X = (int)srcMonster.X + game.Random(rad) - rad / 2;
            p.Y = (int)srcMonster.Y + game.Random(rad) - rad / 2;
            DropTreasureItem(ref itm, p.X, p.Y);

            //any items to drop?
            if (srcMonster.DropNum1 > 0 && srcMonster.DropItem1 != "")
            {
                count = game.Random(1, srcMonster.DropNum1);
                for (int n = 1; n < count; n++)
                {
                    //25% chance for drop
                    if (game.Random(100) < 25)
                    {
                        itm = game.Items.getItem(srcMonster.DropItem1);
                        p.X = (int)srcMonster.X + game.Random(rad) - rad / 2;
                        p.Y = (int)srcMonster.Y + game.Random(rad) - rad / 2;
                        DropTreasureItem(ref itm, p.X, p.Y);
                    }
                }
            }
            if (srcMonster.DropNum2 > 0 && srcMonster.DropItem2 != "")
            {
                count = game.Random(1, srcMonster.DropNum2);
                for (int n = 1; n < count; n++)
                {
                    //25% chance for drop
                    if (game.Random(100) < 25)
                    {
                        itm = game.Items.getItem(srcMonster.DropItem2);
                        p.X = (int)srcMonster.X + game.Random(rad) - rad / 2;
                        p.Y = (int)srcMonster.Y + game.Random(rad) - rad / 2;
                        DropTreasureItem(ref itm, p.X, p.Y);
                    }
                }
            }
            if (srcMonster.DropNum3 > 0 && srcMonster.DropItem3 != "")
            {
                count = game.Random(1, srcMonster.DropNum3);
                for (int n = 1; n < count; n++)
                {
                    //25% chance for drop
                    if (game.Random(100) < 25)
                    {
                        itm = game.Items.getItem(srcMonster.DropItem3);
                        p.X = (int)srcMonster.X + game.Random(rad) - rad / 2;
                        p.Y = (int)srcMonster.Y + game.Random(rad) - rad / 2;
                        DropTreasureItem(ref itm, p.X, p.Y);
                    }
                }
            }
        }

        private void DropTreasureItem(ref Item itm, int x, int y)
        {
            Game.DrawableItem drit;
            drit.item = itm;

            drit.sprite = new Sprite(ref game);
            drit.sprite.Position = new Point(x, y);

            if (drit.item.DropImageFilename == "")
            {
                MessageBox.Show("Error: Item '" + drit.item.Name +
                    "' image file is invalid.");
                Application.Exit();
            }

            drit.sprite.Image = game.LoadBitmap(drit.item.DropImageFilename);
            drit.sprite.Size = drit.sprite.Image.Size;

            game.Treasure.Add(drit);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            game.GameOver = true;
        }

        private void doMonsters()
        {
            PointF relativePos;
            PointF heroCenter;
            PointF monsterCenter;

            heroCenter = game.aysel.CenterPos;

            for (int n = 0; n < farmers.Length; n++)
            {
                if (farmers[n] != null)
                {
                    //is monster in view?
                    if (farmers[n].X > game.map.ScrollPos.X &&
                        farmers[n].X < game.map.ScrollPos.X + 23 * 32 &&
                        farmers[n].Y > game.map.ScrollPos.Y &&
                        farmers[n].Y < game.map.ScrollPos.Y + 17 * 32)
                    {
                        //get relative position on screen
                        relativePos = new PointF(
                            Math.Abs(game.map.ScrollPos.X - farmers[n].X),
                            Math.Abs(game.map.ScrollPos.Y - farmers[n].Y));

                        //get center
                        monsterCenter = relativePos;
                        monsterCenter.X += farmers[n].GetSprite.Width / 2;
                        monsterCenter.Y += farmers[n].GetSprite.Height / 2;

                        //draw the monster sprite
                        farmers[n].Draw(relativePos);
                    }
                }
            }

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
                case Keys.T: talkFlag = true; break;

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
                case Keys.T: talkFlag = false; break;
                case Keys.Space: lootFlag = true; break;
                case Keys.I:
                    game.Inven.Visible = !game.Inven.Visible;
                    break;
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