/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */
   
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AutomationTool.Extension;

namespace AutomationTool.Forms
{
    /// <summary>
    /// Modified By: Sourav Nanda
    /// Modified Date: 11/25/2016
    /// Description: Added comments to the method
    /// </summary>
    public partial class SelectionForm : Form
    {
        #region Properties And Variables
        private DataTable CurrentResultList = null;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public SelectionForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Parameterized constructors.
        /// </summary>
        /// <param name="Caption">Takes caption as window name for the form</param>
        /// <param name="isNarrowWindow">Checking whether it is narrow window or not.</param>
        /// <param name="ShowFilter">Asking to show filter</param>
        /// <param name="MultiSelect">Checking for multiple selection</param>
        public SelectionForm(string Caption, bool isNarrowWindow, bool ShowFilter, bool MultiSelect)
        {
            //  This call is required by the designer.
            InitializeComponent();
            //  Add any initialization after the InitializeComponent() call.
            this.Text = Caption;
            if (isNarrowWindow)
            {
                this.Width = 400;
            }
            else
            {
                this.Width = 600;
            }
            if (ShowFilter)
            {
                FormHolder.TopToolStripPanelVisible = true;
            }
            else
            {
                FormHolder.TopToolStripPanelVisible = false;
            }
            if ((isNarrowWindow && ShowFilter))
            {
                ResultsFilter.Width = 150;
            }
            else
            {
                ResultsFilter.Width = 450;
            }
            ResultsList.MultiSelect = MultiSelect;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Loads List
        /// </summary>
        /// <param name="DataSource">Gets a table of window</param>
        public void LoadList(DataTable DataSource)
        {
            try
            {
                ResultsList.DataSource = DataSource;
                CurrentResultList = DataSource;
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Loads list using List<T>
        /// </summary>
        /// <param name="ListToLoad">gets window in list format</param>
        public void LoadList(ref List<SHDocVw.InternetExplorer> ListToLoad)
        {
            try
            {
                DataTable ListData = new DataTable();
                ListData.Columns.Add("Window Name");

                foreach (SHDocVw.InternetExplorer tmpWindow in ListToLoad)
                {
                    mshtml.IHTMLDocument2 tmpDoc = tmpWindow.Document;
                    string MyTitle = string.Empty;
                    try
                    {
                        MyTitle = tmpDoc.title + " ( " + tmpDoc.domain + " )";
                    }
                    catch
                    {
                        MyTitle = "ERROR: DO NOT SELECT";
                    }
                    ListData.Rows.Add(MyTitle);
                    tmpDoc = null;
                }

                ResultsList.DataSource = null;
                ResultsList.DataSource = ListData;
                ResultsList.Columns["Window Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                ResultsList.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Click event of Make selection.
        /// </summary>
        /// <param name="sender">sender object for the button click</param>
        /// <param name="e">event for button click</param>
        private void MakeSelectionButton_Click(object sender, EventArgs e)
        {
            try
            {
                if ((ResultsList.SelectedRows.Count == 0))
                {
                    //AppActivate(this.Text);
                    // ApiHelper.ActivateWindow(Main.Handle, WindowCommands.SW_SHOW)
                    System.Windows.Forms.MessageBox.Show("You must select a window to continue.", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return;
                }
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }

        /// <summary>
        /// Click event of Cancel.
        /// </summary>
        /// <param name="sender">sender object for the button click</param>
        /// <param name="e">event for button click</param>
        private void CancelSelectionButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
        }
        #endregion
    }
}
