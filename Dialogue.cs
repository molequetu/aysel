using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace Aysel
{
    class Dialogue
    {

        private Game p_game;
        private Font p_fontTitle;
        private Font p_fontMessage;
        private Font p_fontButton;
        private PointF p_position;
        private Positions p_corner;
        private Size p_size;
        private string p_title;
        private string p_message;
        private Button[] p_buttons;
        private int p_numButtons;
        private int p_selection;
        private Point p_mousePos;
        private MouseButtons p_mouseBtn;
        private MouseButtons p_oldMouseBtn;
        private bool p_visible;

        public enum Positions
        {
            UpperLeft,
            LowerLeft,
            UpperRight,
            LowerRight
        }

        public struct Button
        {
            private string text;

            public string Text
            {
                get
                {
                    return text;
                }

                set
                {
                    text = value;
                }
            }
        }

        public Dialogue(ref Game game)
        {
            P_game = game;
            P_corner = Positions.UpperRight;
            P_size = new Size(360, 280);
            P_title = "Title";
            P_message = "Message Text";
            P_fontTitle = new Font("Arial", 20, FontStyle.Regular,
                GraphicsUnit.Pixel);
            P_fontMessage = new Font("Arial", 14, FontStyle.Regular,
                GraphicsUnit.Pixel);
            P_fontButton = new Font("Arial", 12, FontStyle.Regular,
                GraphicsUnit.Pixel);
            P_numButtons = 10;
            P_buttons = new Button[P_numButtons + 1];
            for (int n = 1; n < 11; n++)
                P_buttons[n].Text = "Button " + n.ToString();
            P_selection = 0;
            P_mouseBtn = MouseButtons.None;
            P_oldMouseBtn = P_mouseBtn;
            P_mousePos = new Point(0, 0);
            P_visible = false;
        }

        public string Title
        {
            get { return P_title; }
            set { P_title = value; }
        }

        public string Message
        {
            get { return P_message; }
            set { P_message = value; }
        }

        public int NumButtons
        {
            get { return P_numButtons; }
            set { P_numButtons = value; }
        }

        public void setButtonText(int index, string value)
        {
            P_buttons[index].Text = value;
        }

        public string getButtonText(int index)
        {
            return P_buttons[index].Text;
        }

        public Rectangle getButtonRect(int index)
        {
            int i = index - 1;
            Rectangle rect = new Rectangle((int)P_position.X,
                (int)P_position.Y, 0, 0);
            rect.Width = P_size.Width / 2 - 4;
            rect.Height = (int)(P_size.Height * 0.4 / 5);
            rect.Y += (int)(P_size.Height * 0.6 - 4);
            switch (index)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 9:
                    rect.X += 4;
                    rect.Y += (int)(Math.Floor((double)i / 2) *
                        rect.Height);
                    break;
                case 2:
                case 4:
                case 6:
                case 8:
                case 10:
                    rect.X += 4 + rect.Width;
                    rect.Y += (int)(Math.Floor((double)i / 2) *
                        rect.Height);
                    break;
            }
            return rect;
        }

        public int Selection
        {
            get { return P_selection; }
            set { P_selection = value; }
        }

        //get/set position in pixels 
        public PointF Position
        {
            get { return P_position; }
            set { P_position = value; }
        }

        public bool Visible
        {
            get { return P_visible; }
            set { P_visible = value; }
        }

        internal Game P_game
        {
            get
            {
                return p_game;
            }

            set
            {
                p_game = value;
            }
        }

        public Font P_fontTitle
        {
            get
            {
                return p_fontTitle;
            }

            set
            {
                p_fontTitle = value;
            }
        }

        public Font P_fontMessage
        {
            get
            {
                return p_fontMessage;
            }

            set
            {
                p_fontMessage = value;
            }
        }

        public Font P_fontButton
        {
            get
            {
                return p_fontButton;
            }

            set
            {
                p_fontButton = value;
            }
        }

        public PointF P_position
        {
            get
            {
                return p_position;
            }

            set
            {
                p_position = value;
            }
        }

        internal Positions P_corner
        {
            get
            {
                return p_corner;
            }

            set
            {
                p_corner = value;
            }
        }

        public Size P_size
        {
            get
            {
                return p_size;
            }

            set
            {
                p_size = value;
            }
        }

        public string P_title
        {
            get
            {
                return p_title;
            }

            set
            {
                p_title = value;
            }
        }

        public string P_message
        {
            get
            {
                return p_message;
            }

            set
            {
                p_message = value;
            }
        }

        internal Button[] P_buttons
        {
            get
            {
                return p_buttons;
            }

            set
            {
                p_buttons = value;
            }
        }

        public int P_numButtons
        {
            get
            {
                return p_numButtons;
            }

            set
            {
                p_numButtons = value;
            }
        }

        public int P_selection
        {
            get
            {
                return p_selection;
            }

            set
            {
                p_selection = value;
            }
        }

        public Point P_mousePos
        {
            get
            {
                return p_mousePos;
            }

            set
            {
                p_mousePos = value;
            }
        }

        public MouseButtons P_mouseBtn
        {
            get
            {
                return p_mouseBtn;
            }

            set
            {
                p_mouseBtn = value;
            }
        }

        public MouseButtons P_oldMouseBtn
        {
            get
            {
                return p_oldMouseBtn;
            }

            set
            {
                p_oldMouseBtn = value;
            }
        }

        public bool P_visible
        {
            get
            {
                return p_visible;
            }

            set
            {
                p_visible = value;
            }
        }

        public void setCorner(Positions corner)
        {
            P_corner = corner;
        }

        private void Print(int x, int y, string text, Brush color)
        {
            P_game.Device.DrawString(text, P_fontTitle, color, x, y);
        }

        public void updateMouse(Point mousePos, MouseButtons mouseBtn)
        {
            P_mousePos = mousePos;
            P_oldMouseBtn = P_mouseBtn;
            P_mouseBtn = mouseBtn;
        }

        public void Draw()
        {
            if (!P_visible) return;

            switch (P_corner)
            {
                case Positions.UpperLeft:
                    P_position = new PointF(4, 4);
                    break;
                case Positions.LowerLeft:
                    P_position = new PointF(4, 600 - P_size.Height - 4);
                    break;
                case Positions.UpperRight:
                    P_position = new PointF(800 - P_size.Width - 4, 4);
                    break;
                case Positions.LowerRight:
                    P_position = new PointF(800 - P_size.Width - 4,
                        600 - P_size.Height - 4);
                    break;
            }

            //adjust height to fit buttons snugly
            float height = 180 + ((P_numButtons + 1) / 2) * 20;

            //draw background
            Pen pen = new Pen(Color.FromArgb((int)(255 * 0.6), 50, 50, 50));
            P_game.Device.FillRectangle(pen.Brush, P_position.X,
                P_position.Y, P_size.Width, height);
            P_game.Device.DrawRectangle(Pens.Gold, P_position.X,
                P_position.Y, P_size.Width, height);

            //draw title
            SizeF size = P_game.Device.MeasureString(P_title, P_fontTitle);
            int tx = (int)(P_position.X + P_size.Width / 2 - size.Width / 2);
            int ty = (int)P_position.Y + 6;
            P_game.Device.DrawString(P_title, P_fontTitle, Brushes.Gold,
                tx, ty);

            //draw message text
            SizeF layoutArea = new SizeF(P_size.Width, 120);
            int lines = 4;
            int length = P_message.Length;
            size = P_game.Device.MeasureString(P_message, P_fontMessage,
                layoutArea, null, out length, out lines);
            RectangleF layoutRect = new RectangleF(P_position.X + 4,
                P_position.Y + 34, size.Width, size.Height);
            P_game.Device.DrawString(P_message, P_fontMessage,
                Brushes.White, layoutRect);

            //draw the buttons
            for (int n = 1; n < P_numButtons + 1; n++)
            {
                Rectangle rect = getButtonRect(n);

                //draw button background
                Color color;
                if (rect.Contains(P_mousePos))
                {
                    //clicked on this button?
                    if (P_mouseBtn == MouseButtons.None && P_oldMouseBtn == MouseButtons.Left)
                        P_selection = n;
                    else
                        P_selection = 0;

                    color = Color.FromArgb(200, 80, 100, 120);
                    P_game.Device.FillRectangle(new Pen(color).Brush, rect);
                }

                //draw button border 
                P_game.Device.DrawRectangle(Pens.Gray, rect);

                //print button label 
                size = P_game.Device.MeasureString(P_buttons[n].Text,
                    P_fontButton);
                tx = (int)(rect.X + rect.Width / 2 - size.Width / 2);
                ty = rect.Y + 2;
                P_game.Device.DrawString(P_buttons[n].Text, P_fontButton,
                    Brushes.White, tx, ty);
            }
        }
    }
}

