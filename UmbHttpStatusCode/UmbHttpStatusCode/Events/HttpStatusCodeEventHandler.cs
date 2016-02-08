using System;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Routing;
using Umbraco.Web;

namespace UmbHttpStatusCode.Events
{
    public class HttpStatusCodeEventHandler : ApplicationEventHandler
    {
        /// <summary>
        /// Register event handler on start.
        /// </summary>
        /// <param name="httpApplicationBase">Umbraco application.</param>
        /// <param name="applicationContext">Application context.</param>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            PublishedContentRequest.Prepared += PublishedContentRequest_Prepared;
        }

        /// <summary>
        /// Sets Http status code to value stored in umbHttpStatusCode property.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event properties</param>
        private void PublishedContentRequest_Prepared(object sender, EventArgs e)
        {
            // Get request.
            var request = sender as PublishedContentRequest;
            var context = HttpContext.Current;

            // Ensure request is valid and page exists.  Otherwise, return without doing anything.
            if ((request == null) || (request.Is404) || (request.IsRedirect))
            {
                // Log for debugging.
                LogHelper.Debug<HttpStatusCodeEventHandler>("Stopping HttpStatusCode for requested URL {0} because request was null ({1}), was 404 ({2}), or was a redirect ({3}).",
                    () => context.Request.Url.AbsolutePath,
                    () => (request == null),
                    () => (request.Is404),
                    () => (request.IsRedirect));

                return;
            }

            // Determine if page has umbStatusCode property set.
            if (request.PublishedContent.HasValue("umbHttpStatusCode"))
            {

                // Get status codes.
                var statusCode = request.PublishedContent.GetPropertyValue<int>("umbHttpStatusCode");
                var subStatusCode = (request.PublishedContent.HasValue("umbHttpSubStatusCode")) ? request.PublishedContent.GetPropertyValue<int>("umbHttpSubStatusCode") : 0;

                // If status code is > 0, try to set it.
                if (statusCode > 0)
                {
                    // Set response status for Umbraco, which will handle setting it for IIS.
                    request.SetResponseStatus(statusCode);

                    // Include in try-catch block in case there are problems setting depending on IIS version.
                    try
                    {
                        // Try skipping custom IIS errors if desired.
                        context.Response.TrySkipIisCustomErrors = UmbracoConfig.For.UmbracoSettings().WebRouting.TrySkipIisCustomErrors;

                        // Set substatus code. Umbraco will set main status code.
                        context.Response.SubStatusCode = subStatusCode;
                    }
                    catch { }
                }

                // Log for debugger.
                LogHelper.Debug<HttpStatusCodeEventHandler>("Setting HTTP Status Code {0}.{1} for {2}.",
                    () => statusCode,
                    () => subStatusCode,
                    () => context.Request.Url.AbsolutePath);
            }
        }
    }
}