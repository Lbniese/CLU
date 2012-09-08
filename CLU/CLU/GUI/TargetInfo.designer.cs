namespace CLU.GUI
{
    partial class TargetInfo
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
            this.label1 = new System.Windows.Forms.Label();
            this.tname = new System.Windows.Forms.Label();
            this.tguid = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.distance = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.realDistance = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.facing = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.issafelybehindtarget = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Target name";
            // 
            // tname
            // 
            this.tname.AutoSize = true;
            this.tname.Location = new System.Drawing.Point(148, 13);
            this.tname.Name = "tname";
            this.tname.Size = new System.Drawing.Size(22, 13);
            this.tname.TabIndex = 1;
            this.tname.Text = "xxx";
            // 
            // tguid
            // 
            this.tguid.AutoSize = true;
            this.tguid.Location = new System.Drawing.Point(148, 46);
            this.tguid.Name = "tguid";
            this.tguid.Size = new System.Drawing.Size(22, 13);
            this.tguid.TabIndex = 3;
            this.tguid.Text = "xxx";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Target GUID";
            // 
            // distance
            // 
            this.distance.AutoSize = true;
            this.distance.Location = new System.Drawing.Point(147, 85);
            this.distance.Name = "distance";
            this.distance.Size = new System.Drawing.Size(22, 13);
            this.distance.TabIndex = 5;
            this.distance.Text = "xxx";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 85);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Distance";
            // 
            // realDistance
            // 
            this.realDistance.AutoSize = true;
            this.realDistance.Location = new System.Drawing.Point(148, 123);
            this.realDistance.Name = "realDistance";
            this.realDistance.Size = new System.Drawing.Size(22, 13);
            this.realDistance.TabIndex = 7;
            this.realDistance.Text = "xxx";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(13, 123);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(128, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "Distance to bounding box";
            // 
            // facing
            // 
            this.facing.AutoSize = true;
            this.facing.Location = new System.Drawing.Point(148, 159);
            this.facing.Name = "facing";
            this.facing.Size = new System.Drawing.Size(22, 13);
            this.facing.TabIndex = 9;
            this.facing.Text = "xxx";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(13, 159);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(133, 13);
            this.label10.TabIndex = 8;
            this.label10.Text = "Player facing toward target";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.Timer1Tick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(14, 193);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "IsSafelyBehind";
            // 
            // issafelybehindtarget
            // 
            this.issafelybehindtarget.AutoSize = true;
            this.issafelybehindtarget.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.issafelybehindtarget.Location = new System.Drawing.Point(148, 193);
            this.issafelybehindtarget.Name = "issafelybehindtarget";
            this.issafelybehindtarget.Size = new System.Drawing.Size(26, 16);
            this.issafelybehindtarget.TabIndex = 11;
            this.issafelybehindtarget.Text = "xxx";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(311, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(461, 269);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Current Target Auras";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(15, 236);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(174, 17);
            this.checkBox1.TabIndex = 17;
            this.checkBox1.Text = "Auto Update Target Auras";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(15, 19);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(440, 202);
            this.textBox1.TabIndex = 15;
            this.textBox1.WordWrap = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(362, 232);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 18;
            this.button1.Text = "check";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // TargetInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 297);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.issafelybehindtarget);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.facing);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.realDistance);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.distance);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tguid);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tname);
            this.Controls.Add(this.label1);
            this.Name = "TargetInfo";
            this.Text = "TargetInfo";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label tname;
        private System.Windows.Forms.Label tguid;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label distance;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label realDistance;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label facing;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label issafelybehindtarget;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button1;
    }
}