using System.Windows.Forms;

namespace WindowsForms
{
    public partial class ProgressForm : Form
    {
        public ProgressForm()
        {
            InitializeComponent();
        }

        public void SetTitle(string title)
        {
            this.LabelTitle.Visible = !string.IsNullOrEmpty(title);
            this.LabelTitle.Text = title;
            this.pictureBox1.Enabled = true;
        }
    }
}
