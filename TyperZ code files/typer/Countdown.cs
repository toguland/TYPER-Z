using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace typer
{
    public partial class Countdown : Form
    {
        private Timer countdownTimer;
        private int countdownSeconds;
        private Label lblCountdown;

        public Countdown(int seconds)
        {
            InitializeComponent();

            // Initialize countdownSeconds with the parameter
            countdownSeconds = seconds;

            // Box + Label initialization
            Text = "Countdown";
            Size = new Size(200, 150); 
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // Initialize and configure lblCountdown
            lblCountdown = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 48, FontStyle.Bold),
                Location = new Point(50, 30), 
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblCountdown);

            // Initialize Timer
            countdownTimer = new Timer
            {
                Interval = 1000 // 1 second
            };
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();

            // Update label on load
            lblCountdown.Text = countdownSeconds.ToString();
        }

        // Countdown 3-2-1
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            countdownSeconds--;
            lblCountdown.Text = countdownSeconds > 0 ? countdownSeconds.ToString() : "Go!"; // If else ternary, once timer = 0 : Prints go

            if (countdownSeconds <= 0)
            {
                countdownTimer.Stop();
                Close(); // Close & proceed test
            }
        }
    }
}