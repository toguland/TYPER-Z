using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Diagnostics;

namespace typer
{
    public partial class Form1 : MaterialForm
    {

        private Label paragraphDisplayLabel;
        private Label SpeedLabel;
        private Label AccuracyLabel;
        private TextBox UserInputTextBox;
        private Button btnStartTest;
        private Button btnResetTest;
        private static string executablePath = AppDomain.CurrentDomain.BaseDirectory;
        private string wordlistPath = Path.Combine(executablePath, "(5CHAR) Wordlist.txt");
        private Chart chartWPM;
        private Chart chartAccuracy;
        private Timer timer;
        private DateTime startTime;
        private TimeSpan testDuration = TimeSpan.FromSeconds(10); // Default test duration
        private List<double> accuracyData = new List<double>();
        private List<double> wpmData = new List<double>();
        private List<string> wordList;
        private Random random = new Random();
        AnimatedTheme animatedTheme = new AnimatedTheme();

        private Label lastTestLabel;
        private Label lastTestWPMLabel;
        private Label lastTestAccuracyLabel;
        private Label lastTestDurationLabel;

        private int correctWords;
        private int totalWords;

        private Dictionary<int, TestResult> bestResults = new Dictionary<int, TestResult>();
        private DateTime testDate;

        private double highestWPM = 0;
        private double highestAccuracy = 0;
        private double wpm;
        private double accuracy;


        public class TestResult
        {
            public double WPM { get; set; }
            public double Accuracy { get; set; }
            public TimeSpan Duration { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
            InitializeControls();
            ResetStats();

            animatedTheme = new AnimatedTheme();

            this.DoubleBuffered = true; // double buffering for seamless and smooth graphics


            // Setup timer for animation updates
            Timer animationTimer = new Timer();
            animationTimer.Interval = 50; // Interval between updates
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();


            // Set form properties for full-screen on startup
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None; // Hide border and title bar
            this.Bounds = Screen.PrimaryScreen.Bounds; // Set form bounds to cover the entire screen

            // Create and setup exit button
            Button exitButton = new Button();
            exitButton.Text = "Exit";
            exitButton.BackColor = Color.Transparent; // Transparent background
            exitButton.ForeColor = Color.White;
            exitButton.FlatStyle = FlatStyle.Flat; // Flat appearance
            exitButton.FlatAppearance.BorderSize = 0; // No border
            exitButton.Font = new Font("Arial", 10, FontStyle.Bold);
            exitButton.Size = new Size(60, 30);
            exitButton.Location = new Point(this.Width - exitButton.Width - 20, 20); // Top-right corner

            // Add click event handler for exit button
            exitButton.Click += ExitButton_Click;

            // Add exit button to the form's controls collection
            this.Controls.Add(exitButton);

        }

        // Exit form

        private void ExitButton_Click(object sender, EventArgs e)
        {
            // Close the form
            this.Close();
        }

        // Constantly repaint for no errors
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // repaint
            this.Invalidate();
        }

        // Paint form
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw the animated theme with AnimatedTheme.cs
            animatedTheme.DrawTheme(e.Graphics, this.ClientRectangle);
        }

        private void InitializeControls()
        {

            // Relative positioning, 10x easier than guessing.
            const int startX = 30; // Starting X position
            const int controlSpacing = 15; // Space between controls
            const int buttonWidth = 100; // Button width
            const int buttonHeight = 40; // Button height
            const int chartWidth = 800; // Chart width
            const int chartHeight = 400; // Chart height

            string[] testDurations = { "10", "20", "30", "40", "50", "60" };

            // Word Label
            Label WordLabel = new Label
            {
                Location = new Point(startX, 50),
                AutoSize = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Text = "Select a time to start test:",
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
            };
            Controls.Add(WordLabel);

            // Paragraph Display Label
            paragraphDisplayLabel = new Label
            {
                Name = "paragraphDisplayLabel",
                Location = new Point(startX, WordLabel.Bottom + 80),
                AutoSize = false,
                Width = 1800,
                Height = 175,
                Font = new Font("Arial", 14),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor= Color.White,
            };
            Controls.Add(paragraphDisplayLabel);

            // Reset Button
            btnResetTest = new Button
            {
                Text = "Reset",
                Location = new Point(paragraphDisplayLabel.Location.X + 1750, paragraphDisplayLabel.Location.Y + 235),
                Size = new Size(buttonWidth, buttonHeight),
                Font = new Font("Arial", 12, FontStyle.Bold),
                Enabled = false,
            };
            btnResetTest.Click += btnResetTest_Click;
            Controls.Add(btnResetTest);

            // User Input TextBox
            UserInputTextBox = new TextBox
            {
                Location = new Point(startX, paragraphDisplayLabel.Bottom ),
                Enabled = false,
                Width = 1850,
                Height = 50,
                Font = new Font("Arial", 14),
            };
            UserInputTextBox.TextChanged += UserInputTextBox_TextChanged;
            Controls.Add(UserInputTextBox);

            // Speed Label
            SpeedLabel = new Label
            {
                Location = new Point(startX, UserInputTextBox.Bottom + controlSpacing + 20),
                AutoSize = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };
            Controls.Add(SpeedLabel);

            // Accuracy Label
            AccuracyLabel = new Label
            {
                Location = new Point(startX, SpeedLabel.Bottom + controlSpacing),
                AutoSize = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };
            Controls.Add(AccuracyLabel);

            // WPM Chart
            chartWPM = new Chart
            {
                Location = new Point(startX, AccuracyLabel.Bottom + 80),
                Size = new Size(chartWidth, chartHeight),
                ForeColor = Color.White,
                BackColor = Color.Transparent,

            };
            chartWPM.ChartAreas.Add(new ChartArea());

            // Blend into background
            chartWPM.ChartAreas[0].BackColor = Color.Transparent;
            chartWPM.ChartAreas[0].AxisY.LineColor = Color.Transparent;
            chartWPM.ChartAreas[0].AxisX.LineColor = Color.Transparent;
            chartWPM.ChartAreas[0].AxisX.MinorGrid.LineColor = Color.Transparent;
            chartWPM.ChartAreas[0].AxisY.MinorGrid.LineColor = Color.Transparent;

            chartWPM.Series.Add(new Series("WPM")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = Color.White
            });
            Controls.Add(chartWPM);

            // Accuracy Chart
            chartAccuracy = new Chart
            {
                Location = new Point(startX + chartWidth + controlSpacing, AccuracyLabel.Bottom + 80),
                Size = new Size(chartWidth, chartHeight),
                ForeColor = Color.Transparent,
                BackColor = Color.Transparent,
            };
            chartAccuracy.ChartAreas.Add(new ChartArea());

            // Blend into background
            chartAccuracy.ChartAreas[0].BackColor = Color.Transparent;
            chartAccuracy.ChartAreas[0].AxisY.LineColor = Color.Transparent;
            chartAccuracy.ChartAreas[0].AxisX.LineColor = Color.Transparent;
            chartAccuracy.ChartAreas[0].AxisY.LineColor = Color.Transparent;



            chartAccuracy.Series.Add(new Series("Accuracy")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = Color.White         
            });
            Controls.Add(chartAccuracy);

            // Last Test Label
            lastTestLabel = new Label
            {
                Location = new Point(SpeedLabel.Location.X + 1600, SpeedLabel.Location.Y),
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Text = "Best Test: N/A",
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };
            Controls.Add(lastTestLabel);

            // Last Test WPM Label
            lastTestWPMLabel = new Label
            {
                Location = new Point(SpeedLabel.Location.X + 1600, SpeedLabel.Location.Y + 20),
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Text = "WPM: N/A",
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };
            Controls.Add(lastTestWPMLabel);

            // Last Test Accuracy Label
            lastTestAccuracyLabel = new Label
            {
                Location = new Point(SpeedLabel.Location.X + 1600, SpeedLabel.Location.Y + 40),
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Text = "Accuracy: N/A",
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };
            Controls.Add(lastTestAccuracyLabel);

            // Test duration buttons
            int totalButtonWidth = (buttonWidth + controlSpacing) * testDurations.Length - controlSpacing;
            int startButtonX = (ClientSize.Width - totalButtonWidth) / 2;

            for (int i = 0; i < testDurations.Length; i++) // Loop for button initialization
            {
                Button btn = new Button
                {
                    Text = $"{testDurations[i]} sec",
                    Location = new Point(WordLabel.Location.X + i * (buttonWidth + controlSpacing), WordLabel.Location.Y + 50),
                    Size = new Size(buttonWidth, buttonHeight),
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    Tag = testDurations[i],
                    ForeColor = Color.Black
                    

                };
                btn.Click += (sender, e) =>
                {
                    if (int.TryParse(((Button)sender).Tag.ToString(), out int testDurationSeconds))
                    {
                        // Clear any previous text
                        UserInputTextBox.Clear();
                        // Call StartTest with the selected duration
                        StartTest(TimeSpan.FromSeconds(testDurationSeconds));
                        UserInputTextBox.Enabled = true;
                    }
                    else
                    {
                        // Error handling
                        MessageBox.Show("Invalid test duration.");
                    }
                };

                Controls.Add(btn);
            }
        }

        // Update stats real time based changing text
        private void UserInputTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateStats();
        }
        // Initalize components of test
        private void StartTest(TimeSpan duration)
        {

            // Show countdown dialog
            using (Countdown countdownDialog = new Countdown(3)) // 3 seconds countdown
            {
                countdownDialog.ShowDialog();
            }

            // Reset charts to clear previous test data
            ResetCharts();
            // Reset stats
            ResetStats();
            // Load paragraph on form
            LoadParagraph();

            UserInputTextBox.Enabled = true;
            UserInputTextBox.Focus();

            btnResetTest.Enabled = true;
            startTime = DateTime.Now;
            testDuration = duration; // Update test duration

            // Error handling
            if (timer == null)
            {
                timer = new Timer();
                timer.Interval = 100;
                timer.Tick += Timer_Tick;
            }

            timer.Start();
        }


        // Update form when test has ended
        private void FinishTest()
        {
            timer.Stop();

            // Perform final calculations
            correctWords = CalculateCorrectWords(paragraphDisplayLabel.Text, UserInputTextBox.Text);
            totalWords = paragraphDisplayLabel.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            wpm = CalculateWPM(correctWords, testDuration);
            accuracy = CalculateAccuracy(paragraphDisplayLabel.Text, UserInputTextBox.Text);

            // Store the results for the current test duration
            TestResult currentResult = new TestResult
            {
                WPM = wpm,
                Accuracy = accuracy,
                Duration = testDuration
            };

            // Check if there's a stored best result for this test duration
            if (bestResults.ContainsKey((int)testDuration.TotalSeconds))
            {
                TestResult bestResult = bestResults[(int)testDuration.TotalSeconds];

                // Update best WPM if the current is better
                if (currentResult.WPM > bestResult.WPM)
                {
                    bestResults[(int)testDuration.TotalSeconds].WPM = currentResult.WPM;
                }

                // Update best Accuracy if the current is better
                if (currentResult.Accuracy > bestResult.Accuracy)
                {
                    bestResults[(int)testDuration.TotalSeconds].Accuracy = currentResult.Accuracy;
                }

                // Update best Duration if the current is better
                if (currentResult.Duration < bestResult.Duration)
                {
                    bestResults[(int)testDuration.TotalSeconds].Duration = currentResult.Duration;
                }
            }
            else
            {
                // No existing best result for this duration, add the current result
                bestResults.Add((int)testDuration.TotalSeconds, currentResult);
            }

            // Update form to display best results
            UpdateBestTestLabels();

            // Show results in dialog box after test
            ShowTestResultsDialog();

            // Clear user input and reset for another test
            UserInputTextBox.Clear();
            ResetStats();
            ResetTest();
        }

        private void ShowTestResultsDialog()
        {
            // Calculate statistics real time on user input
            correctWords = CalculateCorrectWords(paragraphDisplayLabel.Text, UserInputTextBox.Text);
            totalWords = paragraphDisplayLabel.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            wpm = CalculateWPM(correctWords, testDuration);
            accuracy = CalculateAccuracy(paragraphDisplayLabel.Text, UserInputTextBox.Text);

            // Debugging output (to console)
            Debug.WriteLine($"Dialog WPM: {wpm}, Dialog Accuracy: {accuracy}");

            // Prepare and show dialog with results
            string message = $"Test Results:\n\n";
            message += $"Words Per Minute (WPM): {wpm:N2}\n";
            message += $"Accuracy: {accuracy:N2}%\n";

            // Test result dialog box
            MessageBox.Show(message, "Test Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        // Timer for test duration(s)
        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;


            // Ends test when reaches testDuration that has been selected
            if (elapsedTime >= testDuration)
            {
                FinishTest();
                UserInputTextBox.Enabled = false;
                btnResetTest.Enabled = false;
            }
            else
            {
                // Calculate WPM based on the elapsed time
                int correctWordsTyped = CalculateCorrectWords(paragraphDisplayLabel.Text, UserInputTextBox.Text); // Correct words
                double currentWPM = CalculateWPM(correctWordsTyped, elapsedTime); // WPM on elapsed time
                SpeedLabel.Text = $"Speed: {currentWPM:N2} WPM";
            }
        }

        private void UpdateStats()
        {
            // Update methods real time based on user input
            correctWords = CalculateCorrectWords(paragraphDisplayLabel.Text, UserInputTextBox.Text);
            totalWords = paragraphDisplayLabel.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            wpm = CalculateWPM(correctWords, testDuration);
            accuracy = CalculateAccuracy(paragraphDisplayLabel.Text, UserInputTextBox.Text);

            // Calculate words typed
            string[] typedWords = UserInputTextBox.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Calculate wpm based on accuracy and how fast they type
            wpm = CalculateWPM(correctWords, testDuration);

            // Calculate accuracy based on accuracy of typed words
            accuracy = CalculateAccuracy(paragraphDisplayLabel.Text, UserInputTextBox.Text);

            // Update labels on form
            SpeedLabel.Text = $"Speed: {wpm:N2} WPM";
            AccuracyLabel.Text = $"Accuracy: {accuracy:N2}%";

            // Add data to lists for chart updates
            wpmData.Add(wpm);
            accuracyData.Add(accuracy);

            // Update charts
            UpdateWPMChart();
            UpdateAccuracyChart();
        }
        // Update WPM graph based on user-input.
        private void UpdateWPMChart()
        {
            // Clear existing points
            chartWPM.Series[0].Points.Clear();

            // Set axis titles
            chartWPM.ChartAreas[0].AxisX.Title = "Time (s)";
            chartWPM.ChartAreas[0].AxisY.Title = "WPM";

            // Add data points
            for (int i = 0; i < wpmData.Count; i++)
            {
                // Calculate X value based on elapsed time and test duration
                double xValue = i * (testDuration.TotalSeconds / wpmData.Count);
                chartWPM.Series[0].Points.AddXY(xValue, wpmData[i]);
            }

            // Adjust X-axis range
            chartWPM.ChartAreas[0].AxisX.Minimum = 0;
            chartWPM.ChartAreas[0].AxisX.Maximum = testDuration.TotalSeconds;
            chartWPM.ChartAreas[0].AxisX.Interval = Math.Max(1, testDuration.TotalSeconds / 10); // Display 10 intervals
            chartWPM.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            chartWPM.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            //Blend for theme
            chartWPM.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.White;
            chartWPM.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.White;

            chartWPM.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            chartWPM.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;

            chartWPM.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            chartWPM.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;

            chartWPM.ChartAreas[0].AxisX.TitleForeColor = Color.White;
            chartWPM.ChartAreas[0].AxisY.TitleForeColor = Color.White;
        }

        private void UpdateAccuracyChart()
        {
            // Clear existing points
            chartAccuracy.Series[0].Points.Clear();

            // Set chart axis properties
            chartAccuracy.ChartAreas[0].AxisX.Title = "Time (s)";
            chartAccuracy.ChartAreas[0].AxisY.Title = "Accuracy (%)";

            // Add data points
            for (int i = 0; i < accuracyData.Count; i++)
            {
                // Calculate X value based on elapsed time and test duration
                double xValue = i * (testDuration.TotalSeconds / accuracyData.Count);
                chartAccuracy.Series[0].Points.AddXY(xValue, accuracyData[i]);
            }

            // Adjust X-axis range
            chartAccuracy.ChartAreas[0].AxisX.Minimum = 0;
            chartAccuracy.ChartAreas[0].AxisX.Maximum = testDuration.TotalSeconds;
            chartAccuracy.ChartAreas[0].AxisX.Interval = Math.Max(1, testDuration.TotalSeconds / 10); // Display 10 intervals


            chartAccuracy.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash; // Simplistic
            chartAccuracy.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            // Blend with theme
            chartAccuracy.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.White;
            chartAccuracy.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.White;

            chartAccuracy.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.White;


            chartAccuracy.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
            chartAccuracy.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;

            chartAccuracy.ChartAreas[0].AxisX.TitleForeColor = Color.White;
            chartAccuracy.ChartAreas[0].AxisY.TitleForeColor = Color.White;
        }

        // Reset charts
        private void ResetCharts()
        {
            // Clear all data points in WPM chart
            while (chartWPM.Series[0].Points.Count > 0)
            {
                chartWPM.Series[0].Points.RemoveAt(0);// remove at 0 or crashes
            }

            // Clear all data points in Accuracy chart
            while (chartAccuracy.Series[0].Points.Count > 0)
            {
                chartAccuracy.Series[0].Points.RemoveAt(0); // remove at 0 or crashes
            }
        }

        // Reset test option
        private void btnResetTest_Click(object sender, EventArgs e) // Reset test
        {
            ResetTest();
        }

        // Reset test logic
        private void ResetTest()
        {
            if (timer != null)
            {
                timer.Stop();
            }

            UserInputTextBox.Text = "";
            UserInputTextBox.Enabled = false; // no input

            // Clear previous data in the lists
            wpmData.Clear();
            accuracyData.Clear();
            // Flush potential input
            UserInputTextBox.Clear();

            // Clear and reset charts
            ResetCharts();

            LoadParagraph(); // Reset the paragraph display
            ResetStats(); // Reset speed and accuracy labels
        }


        // Labels display best test result based on x duration the user has selected (10,20,30,40,50,60).
        private void UpdateBestTestLabels()
        {
            // Clear labels initially
            lastTestLabel.Text = "Best Test: N/A";
            lastTestWPMLabel.Text = "WPM: N/A";
            lastTestAccuracyLabel.Text = "Accuracy: N/A";
            

            foreach (var kvp in bestResults) // via test duration seconds
            {
                int durationSeconds = kvp.Key;
                TestResult result = kvp.Value;

                // Update the corresponding labels based on duration
                lastTestLabel.Text = $"Best Test ({durationSeconds}s):";
                lastTestWPMLabel.Text = $"WPM: {result.WPM:N2}";
                lastTestAccuracyLabel.Text = $"Accuracy: {result.Accuracy:N2}%";
            }
        }

        // Reset labels
        private void ResetStats()
        {
            SpeedLabel.Text = "Speed: 0 WPM";
            AccuracyLabel.Text = "Accuracy: 0%";
        }


        // Calculation of correct wordds by user
        private int CalculateCorrectWords(string originalText, string typedText)
        {
            // Split the text into words and count how many are correct
            string[] originalWords = originalText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); // Split words in an array ,remove whitespace 
            string[] typedWords = typedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); 

            int correctWords = 0;
            for (int i = 0; i < Math.Min(originalWords.Length, typedWords.Length); i++)
            {
                if (originalWords[i] == typedWords[i]) // If typed identical to word, add correct word.
                {
                    correctWords++;
                }
            }

            return correctWords;
        }

        private double CalculateWPM(int correctWordsTyped, TimeSpan elapsedTime) // Speed calculation
        {
            double minutes = elapsedTime.TotalMinutes; // Time of test
            return minutes > 0 ? (correctWordsTyped / minutes) : 0; // Ternary expression, checks if greater than zero; if it is zero or negative, return 0 to avoid error
        }

        // Accuracy calculations
        private double CalculateAccuracy(string originalText, string typedText)
        {
            string[] originalWords = originalText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); // split whitespaces
            string[] typedWords = typedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); // split whitespaces

            int correctWords = 0;
            for (int i = 0; i < Math.Min(originalWords.Length, typedWords.Length); i++) // Compare user typed words with original text
            {
                if (originalWords[i] == typedWords[i])
                {
                    correctWords++;
                }
            }

            int totalWordsTyped = typedWords.Length;

            return totalWordsTyped > 0 ? (double)correctWords / totalWordsTyped * 100 : 0; // Multiply by 100 for percentage
        }

        private List<string> LoadWordList(string filename)
        {
            List<string> wordList = new List<string>(); // Store wordlist as a list internally
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                using (StreamReader sr = new StreamReader(path)) // Read path to write to form.
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        wordList.Add(line.Trim()); // Trim lines
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading word list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Breaks just in case
                Close();
            }
            return wordList;
        }

        private void DisplayRandomWords()
        {
            string displayText = "";
            for (int i = 0; i < 200; i++) // The word count is 400, (Max wpm ~= 400)
            {
                displayText += wordList[random.Next(wordList.Count)] + " "; // Random word from wordlist
            }
            paragraphDisplayLabel.Text = displayText.Trim(); // Remove white-space
        }

        private void LoadParagraph() // Load "paragraph into form (random words)
        {
            wordList = LoadWordList(wordlistPath);

            if (wordList.Count == 0)
            {
                MessageBox.Show("Word list is empty or not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Debug
                return;
            }

            DisplayRandomWords();
        }
    }
}
