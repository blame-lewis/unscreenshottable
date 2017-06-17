using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

/// <summary>
/// The purpose of this program is to draw text in such a way that:
/// A) A single screenshot does not contain any non-random information, and
/// B) A human is still able to read the text (albeit with a little difficulty).
/// </summary>

namespace StaticText
{
    public partial class DemoForm : Form
    {


        /// <summary> The static background from which the two images are generated. </summary>
        Bitmap background;

        /// <summary> One of the images the animation flips between. </summary>
        Bitmap a;

        /// <summary> One of the images the animation flips between. </summary>
        Bitmap b;

        /// <summary> Used to render the output. </summary>
        OpenTK.GLControl glControl;
        
        public DemoForm()
        {
            InitializeComponent();
        }

        /// <summary> Does the core initialization </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            //Generate the black-and-white background
            Random r = new Random();
            background = new Bitmap(panel1.Width, panel1.Height);
            for (int x = 0; x < background.Width; x++)
                for (int y = 0; y < background.Height; y++)
                {
                    int b = r.Next(2) * 255;
                    background.SetPixel(x, y, Color.FromArgb(255, b, b, b));
                }
            panel1.BackgroundImage = background;

            //Do one draw so a and b buffers aren't null
            render();

            //Set up the GL Control
            glControl = new OpenTK.GLControl();
            Controls.Add(glControl);
            glControl.Top = panel1.Bottom;
            glControl.Height = panel1.Height;
            glControl.Left = panel1.Left;
            glControl.Width = panel1.Width;
            glControl.MakeCurrent();
        }

        //The current piece of text being displayed.
        string currentOutput;        

        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)    //They've pressed enter, set up our buffers to display the text
            {
                e.SuppressKeyPress = true;
                currentOutput = inputBox.Text;
                render();
                inputBox.Text = "";
            }
        }

        /// <summary> Renders the selected text and fills our A and B buffers with it </summary>
        void render()
        {
            Bitmap text = new Bitmap(background.Width, background.Height);

            a = new Bitmap(text.Width, text.Height);
            b = new Bitmap(text.Width, text.Height);

            //Draw our string
            Graphics t = Graphics.FromImage(text);
            t.Clear(Color.Black);
            t.DrawString(currentOutput, new Font(FontFamily.GenericSansSerif, 34, FontStyle.Bold), Brushes.White, new PointF(0, 0));
            t.Dispose();
            Random r = new Random();

            //Do the image processing
            for (int x = 0; x < background.Width; x++)
            {
                for (int y = 0; y < background.Height; y++)
                {
                    Color p = background.GetPixel(x, y);

                    Color p2;

                    //If the text has a given pixel, then keep it static between the two buffers.
                    //Otherwise flip brightness between the two (so it inverts every frame for that pixel)

                    if (text.GetPixel(x, y).GetBrightness() < 0.5f)
                        p2 = Color.FromArgb(255, 255 - p.R, 255 - p.G, 255 - p.B);
                    else
                        p2 = p;

                    a.SetPixel(x, y, p);
                    b.SetPixel(x, y, p2);
                }
            }
            //OpenGL loads from the bottom up, so we need to flip them
            a.RotateFlip(RotateFlipType.RotateNoneFlipY);
            b.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }

        /// <summary> Draws whatever bitmap is handed to it through OpenGL.DrawPixels </summary>
        void draw(Bitmap b)
        {
            System.Drawing.Imaging.BitmapData d = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            OpenTK.Graphics.OpenGL.GL.DrawPixels(background.Width, background.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, OpenTK.Graphics.OpenGL.PixelType.UnsignedByte, d.Scan0);
            b.UnlockBits(d);
            glControl.SwapBuffers();
        }
        /// <summary> Chooses between the two buffers </summary>
        bool w = false;

        private void renderTimer_Tick(object sender, EventArgs e)
        {
            //Draw current buffer, then flip it
            if (w)
                draw(a);
            else
                draw(b);
            
            w = !w; 
        }
    }
}
