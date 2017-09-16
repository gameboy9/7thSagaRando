namespace _7thSagaRando
{
    partial class Form1
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
            this.cmdRandomize = new System.Windows.Forms.Button();
            this.lblGPReq = new System.Windows.Forms.Label();
            this.lblExpBoost = new System.Windows.Forms.Label();
            this.lblGoldReq = new System.Windows.Forms.Label();
            this.lblExperience = new System.Windows.Forms.Label();
            this.trkGoldReq = new System.Windows.Forms.TrackBar();
            this.trkExperience = new System.Windows.Forms.TrackBar();
            this.chkStores = new System.Windows.Forms.CheckBox();
            this.chkTreasures = new System.Windows.Forms.CheckBox();
            this.chkHeroStats = new System.Windows.Forms.CheckBox();
            this.chkMonsterPatterns = new System.Windows.Forms.CheckBox();
            this.chkMonsterZones = new System.Windows.Forms.CheckBox();
            this.lblFlags = new System.Windows.Forms.Label();
            this.txtFlags = new System.Windows.Forms.TextBox();
            this.btnNewSeed = new System.Windows.Forms.Button();
            this.lblSeed = new System.Windows.Forms.Label();
            this.txtSeed = new System.Windows.Forms.TextBox();
            this.btnCompareBrowse = new System.Windows.Forms.Button();
            this.lblCompareImage = new System.Windows.Forms.Label();
            this.txtCompare = new System.Windows.Forms.TextBox();
            this.btnCompare = new System.Windows.Forms.Button();
            this.lblReqChecksum = new System.Windows.Forms.Label();
            this.lblRequired = new System.Windows.Forms.Label();
            this.lblSHAChecksum = new System.Windows.Forms.Label();
            this.lblSHA = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblRomImage = new System.Windows.Forms.Label();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.chkWhoCanEquip = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.trkGoldReq)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkExperience)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdRandomize
            // 
            this.cmdRandomize.Location = new System.Drawing.Point(458, 317);
            this.cmdRandomize.Name = "cmdRandomize";
            this.cmdRandomize.Size = new System.Drawing.Size(96, 23);
            this.cmdRandomize.TabIndex = 123;
            this.cmdRandomize.Text = "Randomize";
            this.cmdRandomize.UseVisualStyleBackColor = true;
            this.cmdRandomize.Click += new System.EventHandler(this.cmdRandomize_Click);
            // 
            // lblGPReq
            // 
            this.lblGPReq.AutoSize = true;
            this.lblGPReq.Location = new System.Drawing.Point(28, 322);
            this.lblGPReq.Name = "lblGPReq";
            this.lblGPReq.Size = new System.Drawing.Size(153, 13);
            this.lblGPReq.TabIndex = 122;
            this.lblGPReq.Text = "Randomize Gold Requirements";
            // 
            // lblExpBoost
            // 
            this.lblExpBoost.AutoSize = true;
            this.lblExpBoost.Location = new System.Drawing.Point(28, 299);
            this.lblExpBoost.Name = "lblExpBoost";
            this.lblExpBoost.Size = new System.Drawing.Size(110, 13);
            this.lblExpBoost.TabIndex = 121;
            this.lblExpBoost.Text = "Boost Experience/GP";
            // 
            // lblGoldReq
            // 
            this.lblGoldReq.AutoSize = true;
            this.lblGoldReq.Location = new System.Drawing.Point(393, 322);
            this.lblGoldReq.Name = "lblGoldReq";
            this.lblGoldReq.Size = new System.Drawing.Size(33, 13);
            this.lblGoldReq.TabIndex = 120;
            this.lblGoldReq.Text = "100%";
            // 
            // lblExperience
            // 
            this.lblExperience.AutoSize = true;
            this.lblExperience.Location = new System.Drawing.Point(393, 299);
            this.lblExperience.Name = "lblExperience";
            this.lblExperience.Size = new System.Drawing.Size(33, 13);
            this.lblExperience.TabIndex = 119;
            this.lblExperience.Text = "100%";
            // 
            // trkGoldReq
            // 
            this.trkGoldReq.Enabled = false;
            this.trkGoldReq.Location = new System.Drawing.Point(224, 321);
            this.trkGoldReq.Maximum = 50;
            this.trkGoldReq.Minimum = 10;
            this.trkGoldReq.Name = "trkGoldReq";
            this.trkGoldReq.Size = new System.Drawing.Size(156, 45);
            this.trkGoldReq.TabIndex = 118;
            this.trkGoldReq.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trkGoldReq.Value = 10;
            this.trkGoldReq.Scroll += new System.EventHandler(this.trkGoldReq_Scroll);
            // 
            // trkExperience
            // 
            this.trkExperience.Location = new System.Drawing.Point(224, 298);
            this.trkExperience.Maximum = 50;
            this.trkExperience.Minimum = 5;
            this.trkExperience.Name = "trkExperience";
            this.trkExperience.Size = new System.Drawing.Size(156, 45);
            this.trkExperience.TabIndex = 117;
            this.trkExperience.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trkExperience.Value = 10;
            this.trkExperience.Scroll += new System.EventHandler(this.trkExperience_Scroll);
            // 
            // chkStores
            // 
            this.chkStores.AutoSize = true;
            this.chkStores.Location = new System.Drawing.Point(12, 239);
            this.chkStores.Name = "chkStores";
            this.chkStores.Size = new System.Drawing.Size(112, 17);
            this.chkStores.TabIndex = 111;
            this.chkStores.Text = "Randomize Stores";
            this.chkStores.UseVisualStyleBackColor = true;
            this.chkStores.CheckedChanged += new System.EventHandler(this.determineFlags);
            // 
            // chkTreasures
            // 
            this.chkTreasures.AutoSize = true;
            this.chkTreasures.Enabled = false;
            this.chkTreasures.Location = new System.Drawing.Point(12, 216);
            this.chkTreasures.Name = "chkTreasures";
            this.chkTreasures.Size = new System.Drawing.Size(129, 17);
            this.chkTreasures.TabIndex = 110;
            this.chkTreasures.Text = "Randomize Treasures";
            this.chkTreasures.UseVisualStyleBackColor = true;
            this.chkTreasures.CheckedChanged += new System.EventHandler(this.determineFlags);
            // 
            // chkHeroStats
            // 
            this.chkHeroStats.AutoSize = true;
            this.chkHeroStats.Location = new System.Drawing.Point(12, 193);
            this.chkHeroStats.Name = "chkHeroStats";
            this.chkHeroStats.Size = new System.Drawing.Size(132, 17);
            this.chkHeroStats.TabIndex = 109;
            this.chkHeroStats.Text = "Randomize Hero Stats";
            this.chkHeroStats.UseVisualStyleBackColor = true;
            this.chkHeroStats.CheckedChanged += new System.EventHandler(this.determineFlags);
            // 
            // chkMonsterPatterns
            // 
            this.chkMonsterPatterns.AutoSize = true;
            this.chkMonsterPatterns.Location = new System.Drawing.Point(12, 170);
            this.chkMonsterPatterns.Name = "chkMonsterPatterns";
            this.chkMonsterPatterns.Size = new System.Drawing.Size(162, 17);
            this.chkMonsterPatterns.TabIndex = 108;
            this.chkMonsterPatterns.Text = "Randomize Monster Patterns";
            this.chkMonsterPatterns.UseVisualStyleBackColor = true;
            this.chkMonsterPatterns.CheckedChanged += new System.EventHandler(this.determineFlags);
            // 
            // chkMonsterZones
            // 
            this.chkMonsterZones.AutoSize = true;
            this.chkMonsterZones.Location = new System.Drawing.Point(12, 147);
            this.chkMonsterZones.Name = "chkMonsterZones";
            this.chkMonsterZones.Size = new System.Drawing.Size(153, 17);
            this.chkMonsterZones.TabIndex = 107;
            this.chkMonsterZones.Text = "Randomize Monster Zones";
            this.chkMonsterZones.UseVisualStyleBackColor = true;
            this.chkMonsterZones.CheckedChanged += new System.EventHandler(this.determineFlags);
            // 
            // lblFlags
            // 
            this.lblFlags.AutoSize = true;
            this.lblFlags.Location = new System.Drawing.Point(291, 113);
            this.lblFlags.Name = "lblFlags";
            this.lblFlags.Size = new System.Drawing.Size(32, 13);
            this.lblFlags.TabIndex = 106;
            this.lblFlags.Text = "Flags";
            // 
            // txtFlags
            // 
            this.txtFlags.Location = new System.Drawing.Point(329, 109);
            this.txtFlags.Name = "txtFlags";
            this.txtFlags.Size = new System.Drawing.Size(200, 20);
            this.txtFlags.TabIndex = 105;
            this.txtFlags.TextChanged += new System.EventHandler(this.determineChecks);
            // 
            // btnNewSeed
            // 
            this.btnNewSeed.Location = new System.Drawing.Point(186, 109);
            this.btnNewSeed.Name = "btnNewSeed";
            this.btnNewSeed.Size = new System.Drawing.Size(86, 23);
            this.btnNewSeed.TabIndex = 97;
            this.btnNewSeed.Text = "New Seed";
            this.btnNewSeed.UseVisualStyleBackColor = true;
            this.btnNewSeed.Click += new System.EventHandler(this.btnNewSeed_Click);
            // 
            // lblSeed
            // 
            this.lblSeed.AutoSize = true;
            this.lblSeed.Location = new System.Drawing.Point(12, 113);
            this.lblSeed.Name = "lblSeed";
            this.lblSeed.Size = new System.Drawing.Size(32, 13);
            this.lblSeed.TabIndex = 104;
            this.lblSeed.Text = "Seed";
            // 
            // txtSeed
            // 
            this.txtSeed.Location = new System.Drawing.Point(69, 111);
            this.txtSeed.Name = "txtSeed";
            this.txtSeed.Size = new System.Drawing.Size(100, 20);
            this.txtSeed.TabIndex = 96;
            // 
            // btnCompareBrowse
            // 
            this.btnCompareBrowse.Location = new System.Drawing.Point(458, 33);
            this.btnCompareBrowse.Name = "btnCompareBrowse";
            this.btnCompareBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnCompareBrowse.TabIndex = 94;
            this.btnCompareBrowse.Text = "Browse";
            this.btnCompareBrowse.UseVisualStyleBackColor = true;
            // 
            // lblCompareImage
            // 
            this.lblCompareImage.AutoSize = true;
            this.lblCompareImage.Location = new System.Drawing.Point(12, 35);
            this.lblCompareImage.Name = "lblCompareImage";
            this.lblCompareImage.Size = new System.Drawing.Size(94, 13);
            this.lblCompareImage.TabIndex = 103;
            this.lblCompareImage.Text = "Comparison Image";
            // 
            // txtCompare
            // 
            this.txtCompare.Location = new System.Drawing.Point(132, 35);
            this.txtCompare.Name = "txtCompare";
            this.txtCompare.Size = new System.Drawing.Size(320, 20);
            this.txtCompare.TabIndex = 93;
            // 
            // btnCompare
            // 
            this.btnCompare.Location = new System.Drawing.Point(458, 62);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(75, 23);
            this.btnCompare.TabIndex = 95;
            this.btnCompare.Text = "Compare";
            this.btnCompare.UseVisualStyleBackColor = true;
            // 
            // lblReqChecksum
            // 
            this.lblReqChecksum.AutoSize = true;
            this.lblReqChecksum.Location = new System.Drawing.Point(129, 88);
            this.lblReqChecksum.Name = "lblReqChecksum";
            this.lblReqChecksum.Size = new System.Drawing.Size(238, 13);
            this.lblReqChecksum.TabIndex = 102;
            this.lblReqChecksum.Text = "1c0c6d78bf2bc29160adf48b17ebf5a5bc46230e";
            // 
            // lblRequired
            // 
            this.lblRequired.AutoSize = true;
            this.lblRequired.Location = new System.Drawing.Point(12, 88);
            this.lblRequired.Name = "lblRequired";
            this.lblRequired.Size = new System.Drawing.Size(50, 13);
            this.lblRequired.TabIndex = 101;
            this.lblRequired.Text = "Required";
            // 
            // lblSHAChecksum
            // 
            this.lblSHAChecksum.AutoSize = true;
            this.lblSHAChecksum.Location = new System.Drawing.Point(129, 64);
            this.lblSHAChecksum.Name = "lblSHAChecksum";
            this.lblSHAChecksum.Size = new System.Drawing.Size(247, 13);
            this.lblSHAChecksum.TabIndex = 100;
            this.lblSHAChecksum.Text = "????????????????????????????????????????";
            // 
            // lblSHA
            // 
            this.lblSHA.AutoSize = true;
            this.lblSHA.Location = new System.Drawing.Point(12, 64);
            this.lblSHA.Name = "lblSHA";
            this.lblSHA.Size = new System.Drawing.Size(88, 13);
            this.lblSHA.TabIndex = 99;
            this.lblSHA.Text = "SHA1 Checksum";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(458, 7);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 92;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblRomImage
            // 
            this.lblRomImage.AutoSize = true;
            this.lblRomImage.Location = new System.Drawing.Point(12, 9);
            this.lblRomImage.Name = "lblRomImage";
            this.lblRomImage.Size = new System.Drawing.Size(110, 13);
            this.lblRomImage.TabIndex = 98;
            this.lblRomImage.Text = "7th Saga ROM Image";
            // 
            // txtFileName
            // 
            this.txtFileName.Location = new System.Drawing.Point(132, 9);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(320, 20);
            this.txtFileName.TabIndex = 91;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(19, 363);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 129;
            // 
            // chkWhoCanEquip
            // 
            this.chkWhoCanEquip.AutoSize = true;
            this.chkWhoCanEquip.Location = new System.Drawing.Point(12, 262);
            this.chkWhoCanEquip.Name = "chkWhoCanEquip";
            this.chkWhoCanEquip.Size = new System.Drawing.Size(157, 17);
            this.chkWhoCanEquip.TabIndex = 130;
            this.chkWhoCanEquip.Text = "Randomize Who Can Equip";
            this.chkWhoCanEquip.UseVisualStyleBackColor = true;
            this.chkWhoCanEquip.CheckedChanged += new System.EventHandler(this.determineFlags);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 412);
            this.Controls.Add(this.chkWhoCanEquip);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.cmdRandomize);
            this.Controls.Add(this.lblGPReq);
            this.Controls.Add(this.lblExpBoost);
            this.Controls.Add(this.lblGoldReq);
            this.Controls.Add(this.lblExperience);
            this.Controls.Add(this.trkGoldReq);
            this.Controls.Add(this.trkExperience);
            this.Controls.Add(this.chkStores);
            this.Controls.Add(this.chkTreasures);
            this.Controls.Add(this.chkHeroStats);
            this.Controls.Add(this.chkMonsterPatterns);
            this.Controls.Add(this.chkMonsterZones);
            this.Controls.Add(this.lblFlags);
            this.Controls.Add(this.txtFlags);
            this.Controls.Add(this.btnNewSeed);
            this.Controls.Add(this.lblSeed);
            this.Controls.Add(this.txtSeed);
            this.Controls.Add(this.btnCompareBrowse);
            this.Controls.Add(this.lblCompareImage);
            this.Controls.Add(this.txtCompare);
            this.Controls.Add(this.btnCompare);
            this.Controls.Add(this.lblReqChecksum);
            this.Controls.Add(this.lblRequired);
            this.Controls.Add(this.lblSHAChecksum);
            this.Controls.Add(this.lblSHA);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblRomImage);
            this.Controls.Add(this.txtFileName);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trkGoldReq)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkExperience)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button cmdRandomize;
        private System.Windows.Forms.Label lblGPReq;
        private System.Windows.Forms.Label lblExpBoost;
        private System.Windows.Forms.Label lblGoldReq;
        private System.Windows.Forms.Label lblExperience;
        private System.Windows.Forms.TrackBar trkGoldReq;
        private System.Windows.Forms.TrackBar trkExperience;
        private System.Windows.Forms.CheckBox chkStores;
        private System.Windows.Forms.CheckBox chkTreasures;
        private System.Windows.Forms.CheckBox chkHeroStats;
        private System.Windows.Forms.CheckBox chkMonsterPatterns;
        private System.Windows.Forms.CheckBox chkMonsterZones;
        private System.Windows.Forms.Label lblFlags;
        private System.Windows.Forms.TextBox txtFlags;
        private System.Windows.Forms.Button btnNewSeed;
        private System.Windows.Forms.Label lblSeed;
        private System.Windows.Forms.TextBox txtSeed;
        private System.Windows.Forms.Button btnCompareBrowse;
        private System.Windows.Forms.Label lblCompareImage;
        private System.Windows.Forms.TextBox txtCompare;
        private System.Windows.Forms.Button btnCompare;
        private System.Windows.Forms.Label lblReqChecksum;
        private System.Windows.Forms.Label lblRequired;
        private System.Windows.Forms.Label lblSHAChecksum;
        private System.Windows.Forms.Label lblSHA;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblRomImage;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.CheckBox chkWhoCanEquip;
    }
}

