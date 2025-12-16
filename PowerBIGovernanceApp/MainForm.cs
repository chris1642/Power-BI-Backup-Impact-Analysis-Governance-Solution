using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerBIGovernanceApp
{
    public class MainForm : Form
    {
        // Constants
        private const string DEFAULT_ENVIRONMENT = "Public";
        
        private Panel headerPanel = null!;
        private Label titleLabel = null!;
        private Label subtitleLabel = null!;
        private ComboBox environmentComboBox = null!;
        private Label environmentLabel = null!;
        private Button startButton = null!;
        private RichTextBox logTextBox = null!;
        private ProgressBar progressBar = null!;
        private Label statusLabel = null!;
        private Panel footerPanel = null!;
        private Button openModelButton = null!;

        private readonly string baseFolderPath = @"C:\Power BI Backups";
        private Process? powershellProcess;
        private bool isRunning = false;

        public MainForm()
        {
            InitializeComponent();
            SetupModernStyling();
        }

        private void InitializeComponent()
        {
            this.Text = "Power BI Governance Solution";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 245, 250);

            // Header Panel with gradient
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.FromArgb(0, 120, 212)
            };
            headerPanel.Paint += HeaderPanel_Paint;

            titleLabel = new Label
            {
                Text = "Power BI Governance & Impact Analysis",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(800, 40),
                Location = new Point(20, 20),
                BackColor = Color.Transparent
            };

            subtitleLabel = new Label
            {
                Text = "Automated backup, impact analysis, and governance solution",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(240, 240, 240),
                AutoSize = false,
                Size = new Size(800, 25),
                Location = new Point(20, 65),
                BackColor = Color.Transparent
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);

            // Environment Selection Panel
            Panel configPanel = new Panel
            {
                Location = new Point(30, 140),
                Size = new Size(840, 80),
                BackColor = Color.White
            };
            configPanel.Paint += (s, e) => DrawRoundedPanel(e.Graphics, configPanel, 8);

            environmentLabel = new Label
            {
                Text = "Power BI Environment:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            environmentComboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(20, 45),
                Size = new Size(300, 30)
            };
            environmentComboBox.Items.AddRange(new object[] {
                "Public (Commercial Cloud)",
                "Germany (Microsoft Cloud Germany)",
                "USGov (Azure Government - GCC)",
                "China (Microsoft Cloud China)",
                "USGovHigh (Azure Government - GCC High)",
                "USGovMil (Azure Government - DoD)"
            });
            environmentComboBox.SelectedIndex = 0;

            configPanel.Controls.Add(environmentLabel);
            configPanel.Controls.Add(environmentComboBox);

            // Start Button
            startButton = new Button
            {
                Text = "▶  START GOVERNANCE PROCESS",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(840, 55),
                Location = new Point(30, 235),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            startButton.FlatAppearance.BorderSize = 0;
            startButton.Click += StartButton_Click;
            startButton.MouseEnter += (s, e) => startButton.BackColor = Color.FromArgb(0, 100, 192);
            startButton.MouseLeave += (s, e) => startButton.BackColor = Color.FromArgb(0, 120, 212);

            // Status Label
            statusLabel = new Label
            {
                Text = "Ready to start",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize = true,
                Location = new Point(30, 305)
            };

            // Progress Bar
            progressBar = new ProgressBar
            {
                Location = new Point(30, 330),
                Size = new Size(840, 8),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            // Log Panel
            Panel logPanel = new Panel
            {
                Location = new Point(30, 350),
                Size = new Size(840, 250),
                BackColor = Color.White
            };
            logPanel.Paint += (s, e) => DrawRoundedPanel(e.Graphics, logPanel, 8);

            logTextBox = new RichTextBox
            {
                Location = new Point(10, 10),
                Size = new Size(820, 230),
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.None
            };

            logPanel.Controls.Add(logTextBox);

            // Footer Panel
            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 245)
            };

            openModelButton = new Button
            {
                Text = "📊  Open Power BI Governance Model",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(300, 40),
                Location = new Point(570, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(16, 124, 16),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            openModelButton.FlatAppearance.BorderSize = 0;
            openModelButton.Click += OpenModelButton_Click;
            openModelButton.MouseEnter += (s, e) => { if (openModelButton.Enabled) openModelButton.BackColor = Color.FromArgb(13, 100, 13); };
            openModelButton.MouseLeave += (s, e) => { if (openModelButton.Enabled) openModelButton.BackColor = Color.FromArgb(16, 124, 16); };

            footerPanel.Controls.Add(openModelButton);

            // Add all controls to form
            this.Controls.Add(headerPanel);
            this.Controls.Add(configPanel);
            this.Controls.Add(startButton);
            this.Controls.Add(statusLabel);
            this.Controls.Add(progressBar);
            this.Controls.Add(logPanel);
            this.Controls.Add(footerPanel);
        }

        private void SetupModernStyling()
        {
            // Enable double buffering for smoother rendering
            this.DoubleBuffered = true;
        }

        private void HeaderPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;
            
            using (LinearGradientBrush brush = new LinearGradientBrush(
                panel.ClientRectangle,
                Color.FromArgb(0, 120, 212),
                Color.FromArgb(0, 90, 158),
                LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, panel.ClientRectangle);
            }
        }

        private void DrawRoundedPanel(Graphics g, Panel panel, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = GetRoundedRectangle(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), radius))
            using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
            {
                g.DrawPath(pen, path);
            }
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private async void StartButton_Click(object? sender, EventArgs e)
        {
            if (isRunning)
            {
                MessageBox.Show("A process is already running. Please wait for it to complete.", "Already Running",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                isRunning = true;
                startButton.Enabled = false;
                environmentComboBox.Enabled = false;
                progressBar.Visible = true;
                openModelButton.Enabled = false;
                logTextBox.Clear();

                UpdateStatus("Preparing workspace...");
                await PrepareWorkspace();

                UpdateStatus("Starting Power BI Governance process...");
                await RunPowerShellScript();

                UpdateStatus("Process completed successfully!");
                progressBar.Visible = false;
                openModelButton.Enabled = true;

                MessageBox.Show("Power BI Governance process completed successfully!\n\nYou can now open the Power BI Governance Model to view the results.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                UpdateStatus("Error occurred");
                LogMessage($"ERROR: {ex.Message}", Color.Red);
                MessageBox.Show($"An error occurred:\n\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isRunning = false;
                startButton.Enabled = true;
                environmentComboBox.Enabled = true;
            }
        }

        private async Task PrepareWorkspace()
        {
            await Task.Run(() =>
            {
                // Create base folder
                if (!Directory.Exists(baseFolderPath))
                {
                    Directory.CreateDirectory(baseFolderPath);
                    LogMessage($"✓ Created base folder: {baseFolderPath}");
                }

                // Create Config folder
                string configPath = Path.Combine(baseFolderPath, "Config");
                if (!Directory.Exists(configPath))
                {
                    Directory.CreateDirectory(configPath);
                }

                // Extract embedded resources
                ExtractEmbeddedResource("PowerBIGovernanceApp.Resources.FinalPSScript.txt",
                    Path.Combine(baseFolderPath, "Final PS Script.txt"));
                LogMessage("✓ Extracted PowerShell script");

                ExtractEmbeddedResource("PowerBIGovernanceApp.Resources.PowerBIGovernanceModel.pbit",
                    Path.Combine(baseFolderPath, "Power BI Governance Model.pbit"));
                LogMessage("✓ Extracted Power BI Governance Model");

                // Extract Config files
                ExtractConfigResources(configPath);
                LogMessage("✓ Extracted configuration files");
            });
        }

        private void ExtractEmbeddedResource(string resourceName, string outputPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream? resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    throw new Exception($"Could not find embedded resource: {resourceName}");

                using (FileStream fileStream = File.Create(outputPath))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
        }

        private void ExtractConfigResources(string configPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith("PowerBIGovernanceApp.Resources.Config."));

            foreach (var resourceName in resourceNames)
            {
                var relativePath = resourceName.Replace("PowerBIGovernanceApp.Resources.Config.", "");
                var outputPath = Path.Combine(configPath, relativePath);

                var directory = Path.GetDirectoryName(outputPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                ExtractEmbeddedResource(resourceName, outputPath);
            }
        }

        private async Task RunPowerShellScript()
        {
            string scriptPath = Path.Combine(baseFolderPath, "Final PS Script.txt");
            string environment = GetSelectedEnvironment();

            await Task.Run(() =>
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -NoProfile -File \"{scriptPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = baseFolderPath
                };

                powershellProcess = new Process { StartInfo = psi };

                powershellProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        LogMessage(e.Data);
                    }
                };

                powershellProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        LogMessage($"ERROR: {e.Data}", Color.Red);
                    }
                };

                LogMessage($"Starting PowerShell process with environment: {environment}");
                powershellProcess.Start();

                // Send environment selection to the script
                if (environment != DEFAULT_ENVIRONMENT)
                {
                    powershellProcess.StandardInput.WriteLine(environment);
                }
                else
                {
                    powershellProcess.StandardInput.WriteLine(); // Press Enter for default
                }
                powershellProcess.StandardInput.Flush();

                powershellProcess.BeginOutputReadLine();
                powershellProcess.BeginErrorReadLine();

                powershellProcess.WaitForExit();

                if (powershellProcess.ExitCode != 0)
                {
                    throw new Exception($"PowerShell script exited with code {powershellProcess.ExitCode}");
                }

                LogMessage("✓ PowerShell process completed successfully");
            });
        }

        private string GetSelectedEnvironment()
        {
            if (this.InvokeRequired)
            {
                return (string)this.Invoke(new Func<string>(GetSelectedEnvironment));
            }

            int index = environmentComboBox.SelectedIndex;
            return index switch
            {
                0 => DEFAULT_ENVIRONMENT,
                1 => "Germany",
                2 => "USGov",
                3 => "China",
                4 => "USGovHigh",
                5 => "USGovMil",
                _ => DEFAULT_ENVIRONMENT
            };
        }

        private void OpenModelButton_Click(object? sender, EventArgs e)
        {
            string modelPath = Path.Combine(baseFolderPath, "Power BI Governance Model.pbit");
            
            if (File.Exists(modelPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = modelPath,
                        UseShellExecute = true
                    });
                    LogMessage("✓ Opening Power BI Governance Model...");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening model:\n\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Power BI Governance Model file not found.\n\nPlease run the process first.",
                    "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateStatus(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(UpdateStatus), message);
                return;
            }

            statusLabel.Text = message;
            statusLabel.Refresh();
        }

        private void LogMessage(string message, Color? color = null)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, Color?>(LogMessage), message, color);
                return;
            }

            logTextBox.SelectionStart = logTextBox.TextLength;
            logTextBox.SelectionLength = 0;
            logTextBox.SelectionColor = color ?? Color.Black;
            logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} | {message}\n");
            logTextBox.SelectionColor = logTextBox.ForeColor;
            logTextBox.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isRunning)
            {
                var result = MessageBox.Show("A process is still running. Are you sure you want to exit?",
                    "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                try
                {
                    if (powershellProcess != null && !powershellProcess.HasExited)
                    {
                        powershellProcess.Kill();
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't prevent exit
                    System.Diagnostics.Debug.WriteLine($"Error killing process: {ex.Message}");
                }
            }

            base.OnFormClosing(e);
        }
    }
}
