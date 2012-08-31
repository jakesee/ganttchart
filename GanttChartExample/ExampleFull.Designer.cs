namespace Braincase.GanttChart
{
    partial class ExampleFull
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
            this._mSplitter1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this._mTaskGrid = new System.Windows.Forms.PropertyGrid();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this._mScrollDatePicker = new System.Windows.Forms.DateTimePicker();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this._mNowDatePicker = new System.Windows.Forms.DateTimePicker();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._mStartDatePicker = new System.Windows.Forms.DateTimePicker();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this._mResourceGrid = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.TaskGridView = new System.Windows.Forms.DataGridView();
            this._mChart = new Braincase.GanttChart.Chart();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileNew = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFilePrint = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewDays = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewDaysDayOfWeek = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewDaysDayOfMonth = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewWeek = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewWeeksDayOfMonth = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewWeeksWeekOfYear = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuViewRelationships = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewSlack = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._mSplitter1)).BeginInit();
            this._mSplitter1.Panel1.SuspendLayout();
            this._mSplitter1.Panel2.SuspendLayout();
            this._mSplitter1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TaskGridView)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _mSplitter1
            // 
            this._mSplitter1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mSplitter1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this._mSplitter1.Location = new System.Drawing.Point(0, 24);
            this._mSplitter1.Name = "_mSplitter1";
            // 
            // _mSplitter1.Panel1
            // 
            this._mSplitter1.Panel1.Controls.Add(this.tabControl1);
            // 
            // _mSplitter1.Panel2
            // 
            this._mSplitter1.Panel2.AutoScroll = true;
            this._mSplitter1.Panel2.Controls.Add(this._mChart);
            this._mSplitter1.Size = new System.Drawing.Size(829, 664);
            this._mSplitter1.SplitterDistance = 237;
            this._mSplitter1.SplitterWidth = 5;
            this._mSplitter1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(237, 664);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this._mTaskGrid);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(229, 636);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Properties";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // _mTaskGrid
            // 
            this._mTaskGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mTaskGrid.Location = new System.Drawing.Point(3, 3);
            this._mTaskGrid.Name = "_mTaskGrid";
            this._mTaskGrid.Size = new System.Drawing.Size(223, 630);
            this._mTaskGrid.TabIndex = 2;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox3);
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(229, 636);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Timeline";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this._mScrollDatePicker);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox3.Location = new System.Drawing.Point(3, 106);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(223, 52);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Scroll To Date";
            // 
            // _mScrollDatePicker
            // 
            this._mScrollDatePicker.Dock = System.Windows.Forms.DockStyle.Top;
            this._mScrollDatePicker.Location = new System.Drawing.Point(3, 16);
            this._mScrollDatePicker.Name = "_mScrollDatePicker";
            this._mScrollDatePicker.Size = new System.Drawing.Size(217, 23);
            this._mScrollDatePicker.TabIndex = 1;
            this._mScrollDatePicker.ValueChanged += new System.EventHandler(this._mScrollDatePicker_ValueChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this._mNowDatePicker);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(3, 54);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(223, 52);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Date Now";
            // 
            // _mNowDatePicker
            // 
            this._mNowDatePicker.Dock = System.Windows.Forms.DockStyle.Top;
            this._mNowDatePicker.Location = new System.Drawing.Point(3, 16);
            this._mNowDatePicker.Name = "_mNowDatePicker";
            this._mNowDatePicker.Size = new System.Drawing.Size(217, 23);
            this._mNowDatePicker.TabIndex = 1;
            this._mNowDatePicker.ValueChanged += new System.EventHandler(this._mNowDatePicker_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._mStartDatePicker);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(223, 51);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Project Start Date";
            // 
            // _mStartDatePicker
            // 
            this._mStartDatePicker.Dock = System.Windows.Forms.DockStyle.Top;
            this._mStartDatePicker.Location = new System.Drawing.Point(3, 16);
            this._mStartDatePicker.Name = "_mStartDatePicker";
            this._mStartDatePicker.Size = new System.Drawing.Size(217, 23);
            this._mStartDatePicker.TabIndex = 0;
            this._mStartDatePicker.ValueChanged += new System.EventHandler(this._mDateTimePicker_ValueChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this._mResourceGrid);
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(229, 636);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Resources";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // _mResourceGrid
            // 
            this._mResourceGrid.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName});
            this._mResourceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mResourceGrid.Location = new System.Drawing.Point(3, 3);
            this._mResourceGrid.Name = "_mResourceGrid";
            this._mResourceGrid.Size = new System.Drawing.Size(223, 630);
            this._mResourceGrid.TabIndex = 0;
            this._mResourceGrid.UseCompatibleStateImageBehavior = false;
            this._mResourceGrid.View = System.Windows.Forms.View.List;
            // 
            // colName
            // 
            this.colName.Text = "Name";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.TaskGridView);
            this.tabPage4.Controls.Add(this.label1);
            this.tabPage4.Location = new System.Drawing.Point(4, 24);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(229, 636);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Task List";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // TaskGridView
            // 
            this.TaskGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.TaskGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TaskGridView.Location = new System.Drawing.Point(3, 18);
            this.TaskGridView.Name = "TaskGridView";
            this.TaskGridView.Size = new System.Drawing.Size(223, 615);
            this.TaskGridView.TabIndex = 0;
            this.TaskGridView.SelectionChanged += new System.EventHandler(this.TaskGridView_SelectionChanged);
            // 
            // _mChart
            // 
            this._mChart.AllowTaskDragDrop = false;
            this._mChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mChart.Font = new System.Drawing.Font("Segoe UI", 9F);
            this._mChart.Location = new System.Drawing.Point(0, 0);
            this._mChart.Margin = new System.Windows.Forms.Padding(0);
            this._mChart.Name = "_mChart";
            this._mChart.Padding = new System.Windows.Forms.Padding(5);
            this._mChart.Size = new System.Drawing.Size(587, 664);
            this._mChart.TabIndex = 4;
            this._mChart.TimeScaleDisplay = Braincase.GanttChart.TimeScaleDisplay.DayOfMonth;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(829, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFileNew,
            this.toolStripMenuItem3,
            this.saveToolStripMenuItem,
            this.toolStripMenuItem4,
            this.mnuFilePrint,
            this.toolStripMenuItem1,
            this.mnuFileExit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // mnuFileNew
            // 
            this.mnuFileNew.Name = "mnuFileNew";
            this.mnuFileNew.Size = new System.Drawing.Size(108, 22);
            this.mnuFileNew.Text = "New";
            this.mnuFileNew.Click += new System.EventHandler(this.mnuFileNew_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(105, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(108, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(105, 6);
            // 
            // mnuFilePrint
            // 
            this.mnuFilePrint.Name = "mnuFilePrint";
            this.mnuFilePrint.Size = new System.Drawing.Size(108, 22);
            this.mnuFilePrint.Text = "Print...";
            this.mnuFilePrint.Click += new System.EventHandler(this.mnuFilePrint_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(105, 6);
            // 
            // mnuFileExit
            // 
            this.mnuFileExit.Name = "mnuFileExit";
            this.mnuFileExit.Size = new System.Drawing.Size(108, 22);
            this.mnuFileExit.Text = "Exit";
            this.mnuFileExit.Click += new System.EventHandler(this.mnuFileExit_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuViewDays,
            this.mnuViewWeek,
            this.toolStripMenuItem2,
            this.mnuViewRelationships,
            this.mnuViewSlack});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // mnuViewDays
            // 
            this.mnuViewDays.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuViewDaysDayOfWeek,
            this.mnuViewDaysDayOfMonth});
            this.mnuViewDays.Name = "mnuViewDays";
            this.mnuViewDays.Size = new System.Drawing.Size(144, 22);
            this.mnuViewDays.Text = "Days";
            // 
            // mnuViewDaysDayOfWeek
            // 
            this.mnuViewDaysDayOfWeek.Name = "mnuViewDaysDayOfWeek";
            this.mnuViewDaysDayOfWeek.Size = new System.Drawing.Size(149, 22);
            this.mnuViewDaysDayOfWeek.Text = "Day Of Week";
            this.mnuViewDaysDayOfWeek.Click += new System.EventHandler(this.mnuViewDaysDayOfWeek_Click);
            // 
            // mnuViewDaysDayOfMonth
            // 
            this.mnuViewDaysDayOfMonth.Name = "mnuViewDaysDayOfMonth";
            this.mnuViewDaysDayOfMonth.Size = new System.Drawing.Size(149, 22);
            this.mnuViewDaysDayOfMonth.Text = "Day Of Month";
            this.mnuViewDaysDayOfMonth.Click += new System.EventHandler(this.mnuViewDaysDayOfMonth_Click);
            // 
            // mnuViewWeek
            // 
            this.mnuViewWeek.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuViewWeeksDayOfMonth,
            this.mnuViewWeeksWeekOfYear});
            this.mnuViewWeek.Name = "mnuViewWeek";
            this.mnuViewWeek.Size = new System.Drawing.Size(144, 22);
            this.mnuViewWeek.Text = "Weeks";
            // 
            // mnuViewWeeksDayOfMonth
            // 
            this.mnuViewWeeksDayOfMonth.Name = "mnuViewWeeksDayOfMonth";
            this.mnuViewWeeksDayOfMonth.Size = new System.Drawing.Size(149, 22);
            this.mnuViewWeeksDayOfMonth.Text = "Day Of Month";
            this.mnuViewWeeksDayOfMonth.Click += new System.EventHandler(this.mnuViewWeeksDayOfMonth_Click);
            // 
            // mnuViewWeeksWeekOfYear
            // 
            this.mnuViewWeeksWeekOfYear.Name = "mnuViewWeeksWeekOfYear";
            this.mnuViewWeeksWeekOfYear.Size = new System.Drawing.Size(149, 22);
            this.mnuViewWeeksWeekOfYear.Text = "Week Of Year";
            this.mnuViewWeeksWeekOfYear.Click += new System.EventHandler(this.mnuViewWeeksWeekOfYear_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(141, 6);
            // 
            // mnuViewRelationships
            // 
            this.mnuViewRelationships.Checked = true;
            this.mnuViewRelationships.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuViewRelationships.Name = "mnuViewRelationships";
            this.mnuViewRelationships.Size = new System.Drawing.Size(144, 22);
            this.mnuViewRelationships.Text = "Relationships";
            this.mnuViewRelationships.Click += new System.EventHandler(this.mnuViewRelationships_Click);
            // 
            // mnuViewSlack
            // 
            this.mnuViewSlack.Name = "mnuViewSlack";
            this.mnuViewSlack.Size = new System.Drawing.Size(144, 22);
            this.mnuViewSlack.Text = "Slack";
            this.mnuViewSlack.Click += new System.EventHandler(this.mnuViewSlack_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuHelpAbout});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // mnuHelpAbout
            // 
            this.mnuHelpAbout.Name = "mnuHelpAbout";
            this.mnuHelpAbout.Size = new System.Drawing.Size(171, 22);
            this.mnuHelpAbout.Text = "About Gantt Chart";
            this.mnuHelpAbout.Click += new System.EventHandler(this.mnuHelpAbout_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 688);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(829, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(814, 17);
            this.lblStatus.Spring = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(153, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select a row to scroll to task";
            // 
            // ExampleFull
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(829, 710);
            this.Controls.Add(this._mSplitter1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ExampleFull";
            this.Text = "Gantt Chart";
            this._mSplitter1.Panel1.ResumeLayout(false);
            this._mSplitter1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._mSplitter1)).EndInit();
            this._mSplitter1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TaskGridView)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer _mSplitter1;
        private GanttChart.Chart _mChart;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuFileNew;
        private System.Windows.Forms.ToolStripMenuItem mnuFilePrint;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuFileExit;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuHelpAbout;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem mnuViewDays;
        private System.Windows.Forms.ToolStripMenuItem mnuViewWeek;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.PropertyGrid _mTaskGrid;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DateTimePicker _mStartDatePicker;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripMenuItem mnuViewDaysDayOfWeek;
        private System.Windows.Forms.ToolStripMenuItem mnuViewDaysDayOfMonth;
        private System.Windows.Forms.ToolStripMenuItem mnuViewWeeksDayOfMonth;
        private System.Windows.Forms.ToolStripMenuItem mnuViewWeeksWeekOfYear;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuViewRelationships;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.ListView _mResourceGrid;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ToolStripMenuItem mnuViewSlack;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DateTimePicker _mNowDatePicker;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.DateTimePicker _mScrollDatePicker;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.DataGridView TaskGridView;
        private System.Windows.Forms.Label label1;

    }
}

