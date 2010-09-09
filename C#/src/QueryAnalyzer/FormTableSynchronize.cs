﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Hubble.SQLClient;
using Hubble.Core.DBAdapter;

namespace QueryAnalyzer
{
    public partial class FormTableSynchronize : Form
    {
        internal DbAccess DataAccess { get; set; }

        TableSynchronization _TableSync;
        string _TableName;

        System.Threading.Thread _Thread = null;

        public string TableName
        {
            get
            {
                return _TableName;
            }

            set
            {
                _TableName = value;
                labelTableName.Text = value;
            }
        }

        public FormTableSynchronize()
        {
            InitializeComponent();
        }

        delegate void DelegateShowException(Exception e);
        private void ShowException(Exception e)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new DelegateShowException(ShowException), e);
            }
            else
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }   
    
        delegate void DelegateShowOptimizeProgress(double progress);

        private void ShowOptimizeProgress(double progress)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new DelegateShowOptimizeProgress(ShowOptimizeProgress), progress);
            }
            else
            {
                if (progress >= 100 || progress < 0)
                {
                    progressBar.Value = 100;

                    buttonStart.Enabled = false;

                    labelProgress.Text = string.Format("{0}%", progressBar.Value);
                }
                else
                {
                    progressBar.Value = (int)progress;

                    labelProgress.Text = string.Format("{0}%", progressBar.Value);
                }
            }
        }

        private void ShowProgress()
        {
            double progress = 0;
            while (progress >= 0 && progress < 100)
            {
                try
                {
                    progress = _TableSync.GetProgress();

                    ShowOptimizeProgress(progress);

                    System.Threading.Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    ShowException(e);
                    
                    ShowOptimizeProgress(100);
                    
                    return;
                }
            }

        }

        private bool Start()
        {
            try
            {
                TableSynchronization.OptimizeOption option = TableSynchronization.OptimizeOption.Minimum;

                if (comboBoxOptimizeOption.Text.Equals("Middle"))
                {
                    option = TableSynchronization.OptimizeOption.Middle;
                }

                int step = (int)numericUpDownStep.Value;

                _TableSync = new TableSynchronization(DataAccess.Conn, TableName, step, option);

                _TableSync.Synchronize();

                _Thread = new System.Threading.Thread(ShowProgress);
                _Thread.IsBackground = true;
                _Thread.Start();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool Stop()
        {
            try
            {
                _TableSync.Stop();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (buttonStart.Text == "Start")
            {
                if (Start())
                {
                    buttonStart.Text = "Stop";
                }
            }
            else
            {
                if (Stop())
                {
                    buttonStart.Enabled = false;
                }
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}