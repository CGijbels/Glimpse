using System;
using System.Web;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;

namespace Glimpse.AspNet
{
    public class HttpModule : IHttpModule
    {
        private static readonly object LockObj = new object();
        private static readonly Factory Factory;

        static HttpModule()
        {
            var serviceLocator = new AspNetServiceLocator();
            Factory = new Factory(serviceLocator);
            ILogger logger = Factory.InstantiateLogger();
            serviceLocator.Logger = logger;

            AppDomain.CurrentDomain.SetData(Constants.LoggerKey, logger);
            AppDomain.CurrentDomain.DomainUnload += (sender, e) => OnDomainUnload((AppDomain)sender);
        }

        public static void OnDomainUnload(AppDomain appDomain)
        {
            var logger = appDomain.GetData(Constants.LoggerKey) as ILogger;

            if (logger != null)
            {
                logger.Fatal("App domain with Id: '{0}' and BaseDirectory: '{1}' has been unloaded. Any in memory data stores have been lost.{2}", appDomain.Id, appDomain.BaseDirectory, HttpRuntimeShutdownMessageResolver.ResolveShutdownMessage());
            }
        }

        public void Init(HttpApplication httpApplication)
        {
            Factory.InstantiateLogger().Debug(
                Resources.HttpModuleInitIsCalled,
                this.GetType(),
                this.GetHashCode(),
                httpApplication.GetType(),
                httpApplication.GetHashCode());

            Init(new HttpApplicationWrapper(httpApplication));
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        internal void Init(HttpApplicationBase httpApplication)
        {
            var glimpseRuntimeWrapper = GetRuntime(httpApplication.Application);
            glimpseRuntimeWrapper.Initialize(httpApplication);
        }

        internal GlimpseRuntimeWrapper GetRuntime(HttpApplicationStateBase applicationState)
        {
            var glimpseRuntimeWrapper = applicationState[Constants.RuntimeKey] as GlimpseRuntimeWrapper;

            if (glimpseRuntimeWrapper == null)
            {
                lock (LockObj)
                {
                    glimpseRuntimeWrapper = applicationState[Constants.RuntimeKey] as GlimpseRuntimeWrapper;

                    if (glimpseRuntimeWrapper == null)
                    {
                        glimpseRuntimeWrapper = new GlimpseRuntimeWrapper(Factory.InstantiateFrameworkProvider(), Factory.InstantiateRuntime(), Factory.InstantiateLogger());
                        applicationState.Add(Constants.RuntimeKey, glimpseRuntimeWrapper);

                        Factory.InstantiateLogger().Debug(
                            Resources.HttpModuleInstantiatedGlimpseRuntimeWrapper,
                            glimpseRuntimeWrapper.GetType(),
                            applicationState.GetType(),
                            applicationState.GetHashCode());
                    }
                }
            }

            return glimpseRuntimeWrapper;
        }
    }
}