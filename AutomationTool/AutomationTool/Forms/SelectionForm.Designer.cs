namespace AutomationTool.Forms
{
    partial class SelectionForm
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
            this.ResultsList = new System.Windows.Forms.DataGridView();
            this.FilterItemsLabel = new System.Windows.Forms.ToolStripLabel();
            this.FilterBar = new System.Windows.Forms.ToolStrip();
            this.ResultsFilter = new System.Windows.Forms.ToolStripTextBox();
            this.FormHolder = new System.Windows.Forms.ToolStripContainer();
            this.CancelSelectionButton = new System.Windows.Forms.Button();
            this.MakeSelectionButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ResultsList)).BeginInit();
            this.FilterBar.SuspendLayout();
            this.FormHolder.ContentPanel.SuspendLayout();
            this.FormHolder.TopToolStripPanel.SuspendLayout();
            this.FormHolder.SuspendLayout();
            this.SuspendLayout();
            // 
            // ResultsList
            // 
            this.ResultsList.AllowUserToAddRows = false;
            this.ResultsList.AllowUserToDeleteRows = false;
            this.ResultsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ResultsList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.ResultsList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ResultsList.Location = new System.Drawing.Point(3, 3);
            this.ResultsList.MultiSelect = false;
            this.ResultsList.Name = "ResultsList";
            this.ResultsList.ReadOnly = true;
            this.ResultsList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ResultsList.Size = new System.Drawing.Size(405, 134);
            this.ResultsList.TabIndex = 0;
            // 
            // FilterItemsLabel
            // 
            this.FilterItemsLabel.Name = "FilterItemsLabel";
            this.FilterItemsLabel.Size = new System.Drawing.Size(121, 22);
            this.FilterItemsLabel.Text = "Type here to filter list:";
            // 
            // FilterBar
            // 
            this.FilterBar.Dock = System.Windows.Forms.DockStyle.None;
            this.FilterBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.FilterBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FilterItemsLabel,
            this.ResultsFilter});
            this.FilterBar.Location = new System.Drawing.Point(0, 0);
            this.FilterBar.Name = "FilterBar";
            this.FilterBar.Size = new System.Drawing.Size(408, 25);
            this.FilterBar.Stretch = true;
            this.FilterBar.TabIndex = 0;
            // 
            // ResultsFilter
            // 
            this.ResultsFilter.AutoSize = false;
            this.ResultsFilter.Name = "ResultsFilter";
            this.ResultsFilter.Size = new System.Drawing.Size(150, 25);
            // 
            // FormHolder
            // 
            // 
            // FormHolder.ContentPanel
            // 
            this.FormHolder.ContentPanel.Controls.Add(this.ResultsList);
            this.FormHolder.ContentPanel.Controls.Add(this.CancelSelectionButton);
            this.FormHolder.ContentPanel.Controls.Add(this.MakeSelectionButton);
            this.FormHolder.ContentPanel.Size = new System.Drawing.Size(408, 167);
            this.FormHolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FormHolder.Location = new System.Drawing.Point(0, 0);
            this.FormHolder.Name = "FormHolder";
            this.FormHolder.Size = new System.Drawing.Size(408, 192);
            this.FormHolder.TabIndex = 4;
            this.FormHolder.Text = "ToolStripContainer1";
            // 
            // FormHolder.TopToolStripPanel
            // 
            this.FormHolder.TopToolStripPanel.Controls.Add(this.FilterBar);
            // 
            // CancelSelectionButton
            // 
            this.CancelSelectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelSelectionButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelSelectionButton.Location = new System.Drawing.Point(263, 141);
            this.CancelSelectionButton.Name = "CancelSelectionButton";
            this.CancelSelectionButton.Size = new System.Drawing.Size(142, 23);
            this.CancelSelectionButton.TabIndex = 2;
            this.CancelSelectionButton.Text = "&Cancel";
            this.CancelSelectionButton.UseVisualStyleBackColor = true;
            this.CancelSelectionButton.Click += new System.EventHandler(this.CancelSelectionButton_Click);
            // 
            // MakeSelectionButton
            // 
            this.MakeSelectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.MakeSelectionButton.Location = new System.Drawing.Point(119, 141);
            this.MakeSelectionButton.Name = "MakeSelectionButton";
            this.MakeSelectionButton.Size = new System.Drawing.Size(142, 23);
            this.MakeSelectionButton.TabIndex = 1;
            this.MakeSelectionButton.Text = "&Select";
            this.MakeSelectionButton.UseVisualStyleBackColor = true;
            this.MakeSelectionButton.Click += new System.EventHandler(this.MakeSelectionButton_Click);
            // 
            // SelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 192);
            this.Controls.Add(this.FormHolder);
            this.Name = "SelectionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SelectionForm";
            ((System.ComponentModel.ISupportInitialize)(this.ResultsList)).EndInit();
            this.FilterBar.ResumeLayout(false);
            this.FilterBar.PerformLayout();
            this.FormHolder.ContentPanel.ResumeLayout(false);
            this.FormHolder.TopToolStripPanel.ResumeLayout(false);
            this.FormHolder.TopToolStripPanel.PerformLayout();
            this.FormHolder.ResumeLayout(false);
            this.FormHolder.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.DataGridView ResultsList;
        internal System.Windows.Forms.ToolStripLabel FilterItemsLabel;
        internal System.Windows.Forms.ToolStrip FilterBar;
        internal System.Windows.Forms.ToolStripTextBox ResultsFilter;
        internal System.Windows.Forms.ToolStripContainer FormHolder;
        internal System.Windows.Forms.Button CancelSelectionButton;
        internal System.Windows.Forms.Button MakeSelectionButton;
    }
}