using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace typer
{
    public class AnimatedTheme
    {
        private Timer timer;
        private Color[] colors = {
            Color.FromArgb(0,0,0),
            Color.FromArgb(164, 153, 146),
            Color.FromArgb(71, 71, 71),
            Color.FromArgb(190, 186, 183),
        };
        private Color currentColor;
        private Color nextColor;
        private float blendFactor; // Current blend factor
        private float blendSpeed = 0.02f; // Speed of blending

        public AnimatedTheme()
        {
            currentColor = colors[0];
            nextColor = colors[1];

            timer = new Timer();
            timer.Interval = 50; // Adjust interval as needed for animation smoothness
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update blend factor
            blendFactor += blendSpeed;

            // If blend factor exceeds 1, switch to next color
            if (blendFactor >= 1)
            {
                blendFactor = 0;
                currentColor = nextColor;
                nextColor = GetNextColor();
            }

            // Force form to repaint
            Form form = Application.OpenForms[0];
            form?.Invalidate();
        }

        private Color GetNextColor()
        {
            // Get the index of the next color in the array
            int nextIndex = Array.IndexOf(colors, nextColor) + 1;
            if (nextIndex >= colors.Length)
                nextIndex = 0;

            return colors[nextIndex];
        }



        public void DrawTheme(Graphics graphics, Rectangle clientRectangle)
        {
            // Calculate blended color
            Color blendedColor = BlendColors(currentColor, nextColor, blendFactor);

            // Draw the blended color
            using (SolidBrush brush = new SolidBrush(blendedColor))
            {
                graphics.FillRectangle(brush, clientRectangle);
            }
        }

        private Color BlendColors(Color color1, Color color2, float ratio)
        {
            int r = (int)(color1.R * (1 - ratio) + color2.R * ratio);
            int g = (int)(color1.G * (1 - ratio) + color2.G * ratio);
            int b = (int)(color1.B * (1 - ratio) + color2.B * ratio);
            return Color.FromArgb(r, g, b);
        }
    }
}
