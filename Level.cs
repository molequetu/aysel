﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Xml;

namespace Aysel
{
    class Level
    {   
        // tilemap's structure
        public struct tilemapStruct
        {
            public int tilenum;
            public string data1;
            public string data2;
            public string data3;
            public string data4;
            public bool collidable;
            public bool portal;
            public int portalx;
            public int portaly;
            public string portalfile;
        }

        // tilemap structure array
        private tilemapStruct[] p_tilemap;
        
        // game reference
        private Game p_game;

        // size of map/level in pixels 
        private Size p_mapSize = new Size(0, 0);
        //size of window in tiles
        private Size p_windowSize = new Size(0, 0);

        // tile and its pallete props
        private int p_tileSize;
        private Bitmap p_bmpTiles;
        private int p_columns;

        // scrolling in level
        private Bitmap p_bmpScrollBuffer;
        private Graphics p_gfxScrollBuffer;

        // watch scrolling possition
        private PointF p_scrollPos = new PointF(0, 0);
        private PointF p_subtile = new PointF(0, 0);
        private PointF p_oldScrollPos = new PointF(0, 0);

        private PointF p_oldPlayerPos = new PointF(0, 0);

        private bool p_portalFlag;
        private Point p_portalTarget;
        private bool p_collidableFlag;
        private tilemapStruct p_currentTile;

        public Level(ref Game game, int width, int height, int tileSize)
        {
            p_game = game;
            p_windowSize = new Size(width, height);
            p_mapSize = new Size(width * tileSize, height * tileSize);
            p_tileSize = tileSize;

            //create scroll buffer
            p_bmpScrollBuffer = new Bitmap(p_mapSize.Width + p_tileSize,
                p_mapSize.Height + p_tileSize);
            p_gfxScrollBuffer = Graphics.FromImage(p_bmpScrollBuffer);

            //create tilemap 
            p_tilemap = new tilemapStruct[128 * 128];
        }

        public tilemapStruct getTile(PointF p)
        {
            return getTile((int)(p.Y * 128 + p.X));
        }

        public tilemapStruct getTile(int pixelx, int pixely)
        {
            return getTile(pixely * 128 + pixelx);
        }

        public tilemapStruct getTile(int index)
        {
            return p_tilemap[index];
        }

        //get/set scroll position by whole tile position
        public Point GridPos
        {
            get
            {
                int x = (int)p_scrollPos.X / p_tileSize;
                int y = (int)p_scrollPos.Y / p_tileSize;
                return new Point(x, y);
            }
            set
            {
                float x = value.X * p_tileSize;
                float y = value.Y * p_tileSize;
                p_scrollPos = new PointF(x, y);
            }
        }

        //get/set scroll position by pixel position 
        public PointF ScrollPos
        {
            get { return p_scrollPos; }
            set { p_scrollPos = value; }
        }

        /*
         * Load tilemap from file
         * @param : string filename
         * @returns : true if file exists
         */
        public bool loadTilemap(string filename)
        {
            
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                XmlNodeList nodelist = doc.GetElementsByTagName("tiles");
                foreach (XmlNode node in nodelist)
                {
                    XmlElement element = (XmlElement)node;
                    int index = 0;
                    tilemapStruct ts;
                    string data;

                    //read data fields from xml 
                    data = element.GetElementsByTagName("tile")[0].
                        InnerText;
                    index = Convert.ToInt32(data);
                    data = element.GetElementsByTagName("value")[0].
                        InnerText;
                    ts.tilenum = Convert.ToInt32(data);
                    data = element.GetElementsByTagName("data1")[0].
                        InnerText;
                    ts.data1 = Convert.ToString(data);
                    data = element.GetElementsByTagName("data2")[0].
                        InnerText;
                    ts.data2 = Convert.ToString(data);
                    data = element.GetElementsByTagName("data3")[0].
                        InnerText;
                    ts.data3 = Convert.ToString(data);
                    data = element.GetElementsByTagName("data4")[0].
                        InnerText;
                    ts.data4 = Convert.ToString(data);
                    data = element.GetElementsByTagName("collidable")[0].
                        InnerText;
                    ts.collidable = Convert.ToBoolean(data);
                    data = element.GetElementsByTagName("portal")[0].
                        InnerText;
                    ts.portal = Convert.ToBoolean(data);
                    data = element.GetElementsByTagName("portalx")[0].
                        InnerText;
                    ts.portalx = Convert.ToInt32(data);
                    data = element.GetElementsByTagName("portaly")[0].
                        InnerText;
                    ts.portaly = Convert.ToInt32(data);
                    data = element.GetElementsByTagName("portalfile")[0].
                        InnerText;
                    ts.portalfile = Convert.ToString(data);

                    //store data in tilemap
                    p_tilemap[index] = ts;
                }
            }
            catch (Exception es)
            {
                MessageBox.Show(es.Message);
                return false;
            }
            return true;
        }

        /*
         * Load maps pallete
         * @param : string filename, int columns of image
         * returns : true if filename and columns match
         */
        public bool loadPalette(string filename, int columns)
        {
            p_columns = columns;
            try
            {
                p_bmpTiles = new Bitmap(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }

        /*
         * Calculate scrolling position 
         * and draw scroll buffer
         */
        public void Update()
        {
            int steps = 4;

            p_oldScrollPos = p_scrollPos;
            p_oldPlayerPos = p_game.aysel.Position;

            //up key movement
            if (p_game.keyState.up)
            {
                if (p_game.aysel.Y > 300 - 48)
                {
                    //p_oldPlayerPos = p_game.Hero.Position;
                    p_game.aysel.Y -= steps;
                }
                else
                {
                    //p_oldScrollPos = p_scrollPos;
                    p_scrollPos.Y -= steps;
                    if (p_scrollPos.Y <= 0)
                        p_game.aysel.Y -= steps;
                }
            }
            //down key movement
            else if (p_game.keyState.down)
            {
                if (p_game.aysel.Y < 300 - 48)
                {
                    //p_oldPlayerPos = p_game.Hero.Position;
                    p_game.aysel.Y += steps;
                }
                else
                {
                    //p_oldScrollPos = p_scrollPos;
                    p_scrollPos.Y += steps;
                    if (p_scrollPos.Y >= (127 - 19) * 32)
                        p_game.aysel.Y += steps;
                }
            }

            //left key movement
            if (p_game.keyState.left)
            {
                if (p_game.aysel.X > 400 - 48)
                {
                    //p_oldPlayerPos = p_game.Hero.Position;
                    p_game.aysel.X -= steps;
                }
                else
                {
                    //p_oldScrollPos = p_scrollPos;
                    p_scrollPos.X -= steps;
                    if (p_scrollPos.X <= 0)
                        p_game.aysel.X -= steps;
                }
            }

            //right key movement
            else if (p_game.keyState.right)
            {
                if (p_game.aysel.X < 400 - 48)
                {
                    //p_oldPlayerPos = p_game.Hero.Position;
                    p_game.aysel.X += steps;
                }
                else
                {
                    //p_oldScrollPos = p_scrollPos;
                    p_scrollPos.X += steps;
                    if (p_scrollPos.X >= (127 - 25) * 32)
                        p_game.aysel.X += steps;
                }
            }

            //resolve collidable tile
            Point pos = p_game.aysel.GetCurrentTilePos();
            p_currentTile = getTile(pos.X, pos.Y);
            p_collidableFlag = p_currentTile.collidable;
            if (p_collidableFlag)
            {
                p_game.aysel.Position = p_oldPlayerPos;
                p_scrollPos = p_oldScrollPos;
            }

            //resolve portal tile
            p_portalFlag = p_currentTile.portal;
            if (p_currentTile.portal)
            {
                p_portalTarget = new Point(p_currentTile.portalx -
                    pos.X / 32, p_currentTile.portaly - pos.Y / 32);
            }

            //fill the scroll buffer only when moving
            if (p_scrollPos != p_oldScrollPos)
            {
                p_oldScrollPos = p_scrollPos;

                //validate X range
                if (p_scrollPos.X < 0) p_scrollPos.X = 0;
                if (p_scrollPos.X > (127 - p_windowSize.Width) * p_tileSize)
                    p_scrollPos.X = (127 - p_windowSize.Width) * p_tileSize;

                //validate Y range
                if (p_scrollPos.Y < 0) p_scrollPos.Y = 0;
                if (p_scrollPos.Y > (127 - p_windowSize.Height) * p_tileSize)
                    p_scrollPos.Y = (127 - p_windowSize.Height) * p_tileSize;

                //calculate sub-tile size
                p_subtile.X = p_scrollPos.X % p_tileSize;
                p_subtile.Y = p_scrollPos.Y % p_tileSize;

                //fill scroll buffer with tiles
                fillScrollBuffer();
            }
        }


        private void fillScrollBuffer()
        {
            for (int tx = 0; tx < p_windowSize.Width + 1; tx++)
            {
                for (int ty = 0; ty < p_windowSize.Height + 1; ty++)
                {
                    int sx = (int)p_scrollPos.X / p_tileSize + tx;
                    int sy = (int)p_scrollPos.Y / p_tileSize + ty;
                    int tilenum = p_tilemap[sy * 128 + sx].tilenum;
                    drawTileNumber(tx, ty, tilenum);
                }
            }
        }


        /*
         * Draw the particular tile in 
         * proper position 
         * @param : int row, int collumn, int tile
         */
        public void drawTileNumber(int x, int y, int tile)
        {
            int sx = (tile % p_columns) * (p_tileSize + 1);
            int sy = (tile / p_columns) * (p_tileSize + 1);
            Rectangle src = new Rectangle(sx, sy, p_tileSize, p_tileSize);
            int dx = x * p_tileSize;
            int dy = y * p_tileSize;
            p_gfxScrollBuffer.DrawImage(p_bmpTiles, dx, dy, src,
                GraphicsUnit.Pixel);
        }

        /*
         * Draw level's methods
         */
        public void Draw(Rectangle rect)
        {
            Draw(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public void Draw(int width, int height)
        {
            Draw(0, 0, width, height);
        }

        /*
         * Copy the proper part of the scroll buffer
         * and show it in the window
         * 
         */
        public void Draw(int x, int y, int width, int height)
        {
            Rectangle source = new Rectangle((int)p_subtile.X,
                (int)p_subtile.Y, width, height);
            p_game.Device.DrawImage(p_bmpScrollBuffer, x, y, source,
                GraphicsUnit.Pixel);
        }
    }
}

