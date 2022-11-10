using Allors.Database.Server.Controllers;

namespace Allors.Server
{
    using Allors.Services;

    public class MediaController : BaseMediaController
    {
        public MediaController(ITransactionService transactionService)
            : base(transactionService)
        {
        }

    }
}
