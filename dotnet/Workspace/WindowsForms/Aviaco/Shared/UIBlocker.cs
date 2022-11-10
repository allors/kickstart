namespace WindowsForms
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public class UIBlocker : IDisposable
    {
        private readonly ProgressForm progressForm;
        private readonly Task<DialogResult> progressFormTask;

        public UIBlocker(string message = "Please wait...")
        {
            this.progressForm = new ProgressForm();
            this.progressForm.Cursor = Cursors.WaitCursor;

            this.progressForm.SetTitle(message);
            this.progressForm.StartPosition = FormStartPosition.CenterScreen;

            this.progressFormTask = this.progressForm.ShowDialogAsync();
        }

        public void SetMessage(string message)
        {
            this.progressForm.SetTitle(message);
        }

        public async void Dispose()
        {
            this.progressForm.Dispose();
            await this.progressFormTask;
        }
    }
}