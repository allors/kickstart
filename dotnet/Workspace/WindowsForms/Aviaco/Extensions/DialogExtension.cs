namespace WindowsForms
{
    using System.Threading.Tasks;
    using System.Windows.Forms;

    internal static class DialogExtension
    {
        public static async Task<DialogResult> ShowDialogAsync(this Form @this)
        {
            await Task.Yield();

            if (@this.IsDisposed)
            {
                return DialogResult.OK;
            }

            return @this.ShowDialog();
        }
    }
}