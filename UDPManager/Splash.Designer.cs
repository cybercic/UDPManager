namespace UDPManager
{
    partial class Splash
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Splash));
            timer1 = new System.Windows.Forms.Timer(components);
            timer2 = new System.Windows.Forms.Timer(components);
            timer3 = new System.Windows.Forms.Timer(components);
            lblVersao = new Label();
            SuspendLayout();
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // timer2
            // 
            timer2.Tick += timer2_Tick;
            // 
            // timer3
            // 
            timer3.Tick += timer3_Tick;
            // 
            // lblVersao
            // 
            lblVersao.BackColor = Color.Transparent;
            lblVersao.ForeColor = Color.White;
            lblVersao.Location = new Point(306, 427);
            lblVersao.Margin = new Padding(4, 0, 4, 0);
            lblVersao.Name = "lblVersao";
            lblVersao.Size = new Size(147, 15);
            lblVersao.TabIndex = 2;
            lblVersao.Text = "Versão:  1.4.12345.1234";
            lblVersao.UseWaitCursor = true;
            // 
            // Splash
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.SplashScreen;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(758, 451);
            ControlBox = false;
            Controls.Add(lblVersao);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Splash";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Splash";
            TransparencyKey = Color.Pink;
            UseWaitCursor = true;
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Timer timer3;
        private System.Windows.Forms.Label lblVersao;
    }
}