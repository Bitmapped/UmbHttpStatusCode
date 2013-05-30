using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using umbraco;
using umbraco.cms.businesslogic.web;
using umbraco.NodeFactory;

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
            var node = new Node(e.PageId);

            // Set status if umbHttpStatusCode property exists.
            if (node.HasProperty("umbHttpStatusCode"))
            {
                // Define variables for storing values.
                int statusCode = 0;
                var statusProperty = node.GetProperty("umbHttpStatusCode").Value.ToString();

                if (Int32.TryParse(statusProperty, out statusCode))
                {
                    // Set status code.
                    e.Context.Response.StatusCode = statusCode;

                    // Try setting substatus code if property exists.
                    if (node.HasProperty("umbHttpSubStatusCode"))
                    {
                         // Define variables for storing values.
                        int subStatusCode = 0;
                        var subStatusProperty = node.GetProperty("umbHttpSubStatusCode").Value.ToString();

                        if (Int32.TryParse(subStatusProperty, out subStatusCode))
                        {
                            // Set status code.
                            e.Context.Response.SubStatusCode = subStatusCode;
                        }
                    }
                }
            }
        }
    }
}
