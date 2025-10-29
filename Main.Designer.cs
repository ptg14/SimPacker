namespace SimPacker
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            RTB_log = new RichTextBox();
            G_process = new GroupBox();
            PB_loading = new ProgressBar();
            G_control = new GroupBox();
            CB_obfuscation = new CheckBox();
            LB_pack = new Label();
            LB_unPack = new Label();
            BT_unPack = new Button();
            BT_pack = new Button();
            P_icon = new PictureBox();
            LB_open = new Label();
            BT_open = new Button();
            G_process.SuspendLayout();
            G_control.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)P_icon).BeginInit();
            SuspendLayout();
            // 
            // RTB_log
            // 
            RTB_log.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            RTB_log.Location = new Point(6, 61);
            RTB_log.Name = "RTB_log";
            RTB_log.ReadOnly = true;
            RTB_log.Size = new Size(624, 359);
            RTB_log.TabIndex = 0;
            RTB_log.Text = "";
            // 
            // G_process
            // 
            G_process.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            G_process.Controls.Add(PB_loading);
            G_process.Controls.Add(RTB_log);
            G_process.Location = new Point(12, 12);
            G_process.Name = "G_process";
            G_process.Size = new Size(636, 426);
            G_process.TabIndex = 1;
            G_process.TabStop = false;
            G_process.Text = "Process";
            // 
            // PB_loading
            // 
            PB_loading.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            PB_loading.Location = new Point(6, 26);
            PB_loading.Name = "PB_loading";
            PB_loading.Size = new Size(624, 29);
            PB_loading.TabIndex = 1;
            // 
            // G_control
            // 
            G_control.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            G_control.Controls.Add(CB_obfuscation);
            G_control.Controls.Add(LB_pack);
            G_control.Controls.Add(LB_unPack);
            G_control.Controls.Add(BT_unPack);
            G_control.Controls.Add(BT_pack);
            G_control.Controls.Add(P_icon);
            G_control.Controls.Add(LB_open);
            G_control.Controls.Add(BT_open);
            G_control.Location = new Point(654, 12);
            G_control.Name = "G_control";
            G_control.Size = new Size(134, 420);
            G_control.TabIndex = 2;
            G_control.TabStop = false;
            G_control.Text = "Control";
            // 
            // CB_obfuscation
            // 
            CB_obfuscation.Location = new Point(6, 264);
            CB_obfuscation.Name = "CB_obfuscation";
            CB_obfuscation.Size = new Size(120, 30);
            CB_obfuscation.TabIndex = 8;
            CB_obfuscation.Text = "Obfuscation";
            CB_obfuscation.UseVisualStyleBackColor = true;
            // 
            // LB_pack
            // 
            LB_pack.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LB_pack.Location = new Point(6, 332);
            LB_pack.Name = "LB_pack";
            LB_pack.Size = new Size(122, 25);
            LB_pack.TabIndex = 6;
            LB_pack.Text = "LB_pack";
            // 
            // LB_unPack
            // 
            LB_unPack.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LB_unPack.Location = new Point(6, 392);
            LB_unPack.Name = "LB_unPack";
            LB_unPack.Size = new Size(122, 25);
            LB_unPack.TabIndex = 5;
            LB_unPack.Text = "LB_unPack";
            // 
            // BT_unPack
            // 
            BT_unPack.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BT_unPack.Location = new Point(6, 360);
            BT_unPack.Name = "BT_unPack";
            BT_unPack.Size = new Size(120, 29);
            BT_unPack.TabIndex = 4;
            BT_unPack.Text = "UnPack";
            BT_unPack.UseVisualStyleBackColor = true;
            BT_unPack.Click += BT_unPack_Click;
            // 
            // BT_pack
            // 
            BT_pack.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BT_pack.Location = new Point(6, 300);
            BT_pack.Name = "BT_pack";
            BT_pack.Size = new Size(120, 29);
            BT_pack.TabIndex = 3;
            BT_pack.Text = "Pack";
            BT_pack.UseVisualStyleBackColor = true;
            BT_pack.Click += BT_pack_Click;
            // 
            // P_icon
            // 
            P_icon.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            P_icon.Location = new Point(6, 86);
            P_icon.Name = "P_icon";
            P_icon.Size = new Size(122, 122);
            P_icon.TabIndex = 2;
            P_icon.TabStop = false;
            // 
            // LB_open
            // 
            LB_open.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LB_open.Location = new Point(6, 58);
            LB_open.Name = "LB_open";
            LB_open.Size = new Size(122, 25);
            LB_open.TabIndex = 1;
            LB_open.Text = "LB_open";
            // 
            // BT_open
            // 
            BT_open.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BT_open.Location = new Point(6, 26);
            BT_open.Name = "BT_open";
            BT_open.Size = new Size(122, 29);
            BT_open.TabIndex = 0;
            BT_open.Text = "Open";
            BT_open.UseVisualStyleBackColor = true;
            BT_open.Click += BT_open_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(G_control);
            Controls.Add(G_process);
            Name = "Main";
            Text = "SimPacker";
            G_process.ResumeLayout(false);
            G_control.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)P_icon).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox RTB_log;
        private GroupBox G_process;
        private ProgressBar PB_loading;
        private GroupBox G_control;
        private Button BT_unPack;
        private Button BT_pack;
        private PictureBox P_icon;
        private Label LB_open;
        private Button BT_open;
        private Label LB_pack;
        private Label LB_unPack;
        private CheckBox CB_obfuscation;
    }
}
