﻿namespace USDA_ARS.CoreClient
{
    partial class CoreTest
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
            this.btnPullDataFromAccess = new System.Windows.Forms.Button();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.lblConnectionString = new System.Windows.Forms.Label();
            this.lblAccessTableName = new System.Windows.Forms.Label();
            this.txtAccessTableName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnPullDataFromAccess
            // 
            this.btnPullDataFromAccess.Location = new System.Drawing.Point(28, 183);
            this.btnPullDataFromAccess.Name = "btnPullDataFromAccess";
            this.btnPullDataFromAccess.Size = new System.Drawing.Size(124, 49);
            this.btnPullDataFromAccess.TabIndex = 0;
            this.btnPullDataFromAccess.Text = "Pull Data From Access";
            this.btnPullDataFromAccess.UseVisualStyleBackColor = true;
            this.btnPullDataFromAccess.Click += new System.EventHandler(this.btnPullDataFromAccess_Click);
            // 
            // txtResult
            // 
            this.txtResult.Location = new System.Drawing.Point(261, 6);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResult.Size = new System.Drawing.Size(2881, 500);
            this.txtResult.TabIndex = 1;
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Location = new System.Drawing.Point(13, 32);
            this.txtConnectionString.Multiline = true;
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtConnectionString.Size = new System.Drawing.Size(212, 82);
            this.txtConnectionString.TabIndex = 2;
            // 
            // lblConnectionString
            // 
            this.lblConnectionString.AutoSize = true;
            this.lblConnectionString.Location = new System.Drawing.Point(0, 9);
            this.lblConnectionString.Name = "lblConnectionString";
            this.lblConnectionString.Size = new System.Drawing.Size(136, 20);
            this.lblConnectionString.TabIndex = 3;
            this.lblConnectionString.Text = "Connection String";
            // 
            // lblAccessTableName
            // 
            this.lblAccessTableName.AutoSize = true;
            this.lblAccessTableName.Location = new System.Drawing.Point(9, 117);
            this.lblAccessTableName.Name = "lblAccessTableName";
            this.lblAccessTableName.Size = new System.Drawing.Size(94, 20);
            this.lblAccessTableName.TabIndex = 5;
            this.lblAccessTableName.Text = "Table Name";
            // 
            // txtAccessTableName
            // 
            this.txtAccessTableName.Location = new System.Drawing.Point(13, 149);
            this.txtAccessTableName.Multiline = true;
            this.txtAccessTableName.Name = "txtAccessTableName";
            this.txtAccessTableName.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtAccessTableName.Size = new System.Drawing.Size(189, 19);
            this.txtAccessTableName.TabIndex = 4;
            // 
            // CoreTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 244);
            this.Controls.Add(this.lblAccessTableName);
            this.Controls.Add(this.txtAccessTableName);
            this.Controls.Add(this.lblConnectionString);
            this.Controls.Add(this.txtConnectionString);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.btnPullDataFromAccess);
            this.Name = "CoreTest";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPullDataFromAccess;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Label lblConnectionString;
        private System.Windows.Forms.Label lblAccessTableName;
        private System.Windows.Forms.TextBox txtAccessTableName;
    }
}

