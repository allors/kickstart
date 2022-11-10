using Allors.Database.Domain;
using Allors.Database.Meta;
using Allors.Database.Server.Controllers;
using WorkTask = Allors.Database.Domain.WorkTask;

namespace Allors.Server
{
    using System;
    using System.Linq;
    using Allors;
    using Allors.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Net.Http.Headers;
    using SkiaSharp;

    public class MediaController : BaseMediaController
    {
        public MediaController(ITransactionService transactionService)
            : base(transactionService)
        {
        }

        [Authorize]
        [AllowAnonymous]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = OneYearInSeconds)]
        [HttpGet("/allors/process/{idString}/{revisionString}/{*name}")]
        public virtual IActionResult Process(string idString, string revisionString, string name)
        {
            if (Guid.TryParse(idString, out var id))
            {
                var m = this.Transaction.Database.Services.Get<MetaPopulation>();

                var media = new Medias(this.Transaction).FindBy(m.Media.UniqueId, id);
                if (media != null)
                {
                    if (media.MediaContent?.Data == null)
                    {
                        return this.NoContent();
                    }

                    if (Guid.TryParse(revisionString, out var revision))
                    {
                        if (media.Revision != revision)
                        {
                            return this.RedirectToAction(nameof(RedirectOrNotFound), new { idString, name });
                        }
                    }
                    else
                    {
                        return this.RedirectToAction(nameof(RedirectOrNotFound), new { idString, name });
                    }

                    // Use Etags
                    this.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var requestEtagValues);
                    if (requestEtagValues != StringValues.Empty)
                    {
                        var etagValueString = requestEtagValues.FirstOrDefault()?.Replace("\"", string.Empty);
                        if (Guid.TryParse(etagValueString, out var etagValue))
                        {
                            if (media.Revision.Equals(etagValue))
                            {
                                this.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status304NotModified;
                                this.Response.ContentLength = 0L;
                                return this.Content(string.Empty);
                            }
                        }
                    }

                    this.Response.Headers[HeaderNames.ETag] = $"\"{media.Revision}\"";

                    var processed = media.Processed;
                    if (processed == null)
                    {
                        var data = media.MediaContent.Data;
                        var mediaType = media.MediaContent.Type ?? "application/octet-stream";
                        return this.File(data, mediaType, name ?? media.FileName);
                    }
                    else
                    {
                        using var image = SKImage.FromBitmap(processed);
                        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                        return this.File(data.ToArray(), "image/png", media.Name + ".png");
                    }

                }
            }

            return this.NotFound("Media with id " + id + " not found.");
        }
        
        [Authorize]
        [AllowAnonymous]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("/allors/printforworker/{idString}/{*name}")]
        public virtual ActionResult Print(string idString, string name)
        {
            if (this.Transaction.Instantiate(idString) is WorkTask workTask)
            {
                if (workTask.PrintWorkerDocument?.ExistMedia == false)
                {
                    workTask.PrintForWorker();
                    this.Transaction.Derive();
                    this.Transaction.Commit();
                }

                var media = workTask.PrintWorkerDocument?.Media;

                if (media == null)
                {
                    return this.NoContent();
                }


                return this.RedirectToAction(nameof(Get), new { idString = media.UniqueId.ToString("N"), revisionString = media.Revision?.ToString("N"), name });
            }

            return this.NotFound("Printable with id " + idString + " not found.");
        }

    }
}
