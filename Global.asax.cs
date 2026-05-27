using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using RPACProductionPlanner.App_Start;

namespace RPACProductionPlanner
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DependencyResolver.SetResolver(new SimpleDependencyResolver());
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();

            // Walk the exception chain looking for an anti-forgery failure
            bool isAntiForgeryException = false;
            var currentEx = exception;
            while (currentEx != null)
            {
                if (currentEx is HttpAntiForgeryException)
                {
                    isAntiForgeryException = true;
                    break;
                }
                currentEx = currentEx.InnerException;
            }

            if (isAntiForgeryException)
            {
                Server.ClearError();
                System.Web.Security.FormsAuthentication.SignOut();

                if (Context.Session != null)
                {
                    Context.Session.Clear();
                    Context.Session.Abandon();
                }

                Response.Redirect("~/Account/Login");
            }
        }
    }
}
