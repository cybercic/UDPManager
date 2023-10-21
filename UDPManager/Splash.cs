namespace UDPManager
{
    public partial class Splash : Form
    {
        public Splash()
        {
            InitializeComponent();

            lblVersao.Text = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

            Opacity = 0;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Opacity == 1)
            {
                timer2.Start();
                timer1.Stop();
            }
            else
                Opacity += 0.05;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();
            Refresh();

            timer3.Start();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (Opacity == 0)
            {
                timer3.Stop();

                Close();
            }
            else
                Opacity -= 0.05;
        }
    }
}
