using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using umbraco;
using umbraco.cms.businesslogic.web;

namespace UmbHttpStatusCode.Events
{
    public class HttpStatusCodeEventHandler : IApplicationEventHandler
    {
        // Ensure event handler is only registered once.
        private static object registerLock = new object();
        private static bool registerRan = false;

        public void OnApplicationStarted(UmbracoApplicationBase httpApplicationBase, ApplicationContext applicationContext)
        {
            // Handle locking.
            if (!registerRan)
            {
                lock (registerLock)
                {
                    if (!registerRan)
                    {
                        // Register event.
                        UmbracoDefault.AfterRequestInit += new UmbracoDefault.RequestInitEventHandler(this.SetHttpStatusCode);



                        // Record that registration happened.
                        registerRan = true;
                    }
                }
            }
        }

        #region Unused interface methods
        public void OnApplicationStarting(UmbracoApplicationBase httpApplicationBase, ApplicationContext applicationContext) { }
        public void OnApplicationInitialized(UmbracoApplicationBase httpApplicationBase, ApplicationContext applicationContext) { }
        #endregion

        /// <summary>
        /// Sets Http status code to value stored in umbHttpStatusCode property.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event properties</param>
        private void SetHttpStatusCode(object sender, RequestInitEventArgs e)
        {
            // Ensure there is a page to load.
            if (e.Page == null)
            {
                // No valid page.  Do nothing.
                return;
            }

            // Load document corresponding with current page.
            var document = new Document(e.Page.PageID);

            // Set status if umbHttpStatusCode property exists.
            var statusProperty = document.getProperty("umbHttpStatusCode");
            if (statusProperty != null)
            {
                // Set status code.
                e.Context.Response.StatusCode = Convert.ToInt32(statusProperty.Value);

                // Look for status subcode.
                var substatusProperty = document.getProperty("umbHttpSubStatusCode");
                if (substatusProperty != null)
                {
                    // Set substatus code.
                    e.Context.Response.SubStatusCode = Convert.ToInt32(substatusProperty.Value);
                }
            }
        }
    }
}
