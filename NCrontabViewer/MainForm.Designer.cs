namespace NCrontabViewer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._startTimePicker = new System.Windows.Forms.DateTimePicker();
            this._endTimePicker = new System.Windows.Forms.DateTimePicker();
            this._timer = new System.Windows.Forms.Timer(this.components);
            this._resultBox = new System.Windows.Forms.RichTextBox();
            this._cronBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._moreButton = new System.Windows.Forms.Button();
            this._statusBar = new System.Windows.Forms.StatusBar();
            this._statusBarPanel = new System.Windows.Forms.StatusBarPanel();
            this._errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this._statusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // _startTimePicker
            // 
            this._startTimePicker.CustomFormat = "dd/MM/yyyy HH:mm";
            this._startTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._startTimePicker.Location = new System.Drawing.Point(65, 16);
            this._startTimePicker.Name = "_startTimePicker";
            this._startTimePicker.Size = new System.Drawing.Size(166, 27);
            this._startTimePicker.TabIndex = 0;
            this._startTimePicker.ValueChanged += new System.EventHandler(this.CronBox_Changed);
            // 
            // _endTimePicker
            // 
            this._endTimePicker.CustomFormat = "dd/MM/yyyy HH:mm";
            this._endTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this._endTimePicker.Location = new System.Drawing.Point(298, 16);
            this._endTimePicker.Name = "_endTimePicker";
            this._endTimePicker.Size = new System.Drawing.Size(166, 27);
            this._endTimePicker.TabIndex = 1;
            this._endTimePicker.ValueChanged += new System.EventHandler(this.CronBox_Changed);
            // 
            // _timer
            // 
            this._timer.Enabled = true;
            this._timer.Interval = 500;
            this._timer.Tick += new System.EventHandler(this.Timer_Tick);
            // 
            // _resultBox
            // 
            this._resultBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._resultBox.BackColor = System.Drawing.SystemColors.Control;
            this._resultBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._resultBox.Location = new System.Drawing.Point(16, 88);
            this._resultBox.Name = "_resultBox";
            this._resultBox.ReadOnly = true;
            this._resultBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this._resultBox.Size = new System.Drawing.Size(531, 344);
            this._resultBox.TabIndex = 3;
            this._resultBox.Text = "";
            // 
            // _cronBox
            // 
            this._cronBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._cronBox.Location = new System.Drawing.Point(143, 48);
            this._cronBox.Name = "_cronBox";
            this._cronBox.Size = new System.Drawing.Size(323, 27);
            this._cronBox.TabIndex = 2;
            this._cronBox.Text = "* * * * *";
            this._cronBox.TextChanged += new System.EventHandler(this.CronBox_Changed);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "&Start:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(255, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "&End:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(126, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "&Recurring Pattern:";
            // 
            // _moreButton
            // 
            this._moreButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._moreButton.Enabled = false;
            this._moreButton.Location = new System.Drawing.Point(484, 48);
            this._moreButton.Name = "_moreButton";
            this._moreButton.Size = new System.Drawing.Size(63, 27);
            this._moreButton.TabIndex = 7;
            this._moreButton.Text = "&More";
            this._moreButton.Click += new System.EventHandler(this.More_Click);
            // 
            // _statusBar
            // 
            this._statusBar.Location = new System.Drawing.Point(0, 448);
            this._statusBar.Name = "_statusBar";
            this._statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this._statusBarPanel});
            this._statusBar.ShowPanels = true;
            this._statusBar.Size = new System.Drawing.Size(563, 24);
            this._statusBar.TabIndex = 8;
            this._statusBar.Text = "Ready";
            // 
            // _statusBarPanel
            // 
            this._statusBarPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this._statusBarPanel.Name = "_statusBarPanel";
            this._statusBarPanel.Text = "Ready";
            this._statusBarPanel.Width = 542;
            // 
            // _errorProvider
            // 
            this._errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.AlwaysBlink;
            this._errorProvider.ContainerControl = this;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(563, 472);
            this.Controls.Add(this._statusBar);
            this.Controls.Add(this._moreButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._cronBox);
            this.Controls.Add(this._resultBox);
            this.Controls.Add(this._startTimePicker);
            this.Controls.Add(this._endTimePicker);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MainForm";
            this.Text = "Crontab Viewer";
            ((System.ComponentModel.ISupportInitialize)(this._statusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker _startTimePicker;
        private System.Windows.Forms.DateTimePicker _endTimePicker;
        private System.Windows.Forms.Timer _timer;
        private System.Windows.Forms.RichTextBox _resultBox;
        private System.Windows.Forms.TextBox _cronBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button _moreButton;
        private System.Windows.Forms.StatusBar _statusBar;
        private System.Windows.Forms.StatusBarPanel _statusBarPanel;
        private System.Windows.Forms.ErrorProvider _errorProvider;
    }
}