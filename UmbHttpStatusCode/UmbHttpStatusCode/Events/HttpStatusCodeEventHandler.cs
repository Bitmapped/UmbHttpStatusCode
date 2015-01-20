using System;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Routing;

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
            PublishedContentRequest request = sender as PublishedContentRequest;
            HttpContext context = HttpContext.Current;

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
            if (request.PublishedContent.GetProperty("umbHttpStatusCode") != null)
            {
                // Define variables for storing values.
                int statusCode = 0;
                int subStatusCode = 0;
                var statusProperty = request.PublishedContent.GetProperty("umbHttpStatusCode").Value.ToString();

                // Try to parse status code.  If valid and greater than 0, set response code.
                if (Int32.TryParse(statusProperty, out statusCode) && (statusCode > 0))
                {
                    // Set status code for IIS.
                    context.Response.StatusCode = statusCode;

                    // Set status code for Umbraco.
                    request.SetResponseStatus(statusCode);

                    // Try skipping custom IIS errors if desired.
                    context.Response.TrySkipIisCustomErrors = UmbracoConfig.For.UmbracoSettings().WebRouting.TrySkipIisCustomErrors;

                    // Try setting substatus code if property exists.
                    if (request.PublishedContent.GetProperty("umbHttpSubStatusCode") != null)
                    {
                        // Define variables for storing values.
                        var subStatusProperty = request.PublishedContent.GetProperty("umbHttpSubStatusCode").Value.ToString();

                        // Try to parse substatus code.  If valid and greater than 0, set response code.
                        if (Int32.TryParse(subStatusProperty, out subStatusCode) && (subStatusCode > 0))
                        {
                            try
                            {
                                // Set status code.
                                context.Response.SubStatusCode = subStatusCode;
                            }
                            catch { }
                        }
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
}