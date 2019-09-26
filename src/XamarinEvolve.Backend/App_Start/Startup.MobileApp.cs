using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using XamarinEvolve.DataObjects;
using XamarinEvolve.Backend.Models;
using Owin;
using Microsoft.Azure.Mobile.Server;
using WebApiThrottle;
using XamarinEvolve.Backend.Helpers;
using System.Web.Http.ExceptionHandling;
using XamarinEvolve.Backend.App_Start;
using Microsoft.Azure.Mobile.Server.Authentication;
using System.Configuration;

namespace XamarinEvolve.Backend
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            app.UseMobileAppRequestHandler();
			//to enable auth, you need to set the app service to use authentication. then set this to anonymous
			// and enable the tokenstore. this then enables custom auth handler to provide a token that cna be used against
			// any of the [authorize] protected method calls. Not configuring this, will
			// result in 401 auth errors even if you just obtained a valid new token!
			if (FeatureFlags.LoginEnabled)
            {
                config.Routes.MapHttpRoute("XamarinAuthProvider", ".auth/login/xamarin", new { controller = "XamarinAuth" });
            }
            else
            {
                config.Routes.MapHttpRoute("AnonymousUserAuthProvider", ".auth/login/anonymoususer", new { controller = "AnonymousUserAuth" });
            }
            //For more information on Web API tracing, see http://go.microsoft.com/fwlink/?LinkId=620686 
            config.EnableSystemDiagnosticsTracing();

            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .ApplyTo(config);

            
            // Use Entity Framework Code First to create database tables based on your DbContext
            Database.SetInitializer(new XamarinEvolveContextInitializer());

            // To prevent Entity Framework from modifying your database schema, use a null database initializer
            // Database.SetInitializer(null);

            MobileAppSettingsDictionary settings = config.GetMobileAppSettingsProvider().GetMobileAppSettings();

            // This middleware is intended to be used locally for debugging. By default, HostName will
            // only have a value when running in an App Service application.
    //        if (string.IsNullOrEmpty(settings.HostName))
    //        {
    //            var options = new Microsoft.Azure.Mobile.Server.Authentication.AppServiceAuthenticationOptions
    //            {
				//	SigningKey = ConfigurationManager.AppSettings["MS_SigningKey"],
				//	ValidAudiences = new[] { ConfigurationManager.AppSettings["MS_MobileServiceName"] },
				//	ValidIssuers = new[] { ConfigurationManager.AppSettings["MS_MobileServiceName"] },
				//	TokenHandler = config.GetAppServiceTokenHandler()
				//};
				//app.UseAppServiceAuthentication(options);
		  //  }          
		    app.UseWebApi(config);
            ConfigureSwagger(config);
 
        }
    }
}

