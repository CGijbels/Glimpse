using System;
using System.IO;
using System.Web;
using Glimpse.AspNet;
using Glimpse.AspNet.Extensions;
using Glimpse.Core.Framework;
using Glimpse.Test.AspNet.Tester;
using Glimpse.Test.Common;
using Xunit;
using Moq;

namespace Glimpse.Test.AspNet
{
    public class HttpModuleShould:IDisposable
    {
        private HttpModuleTester tester;

        private HttpModuleTester HttpModule
        {
            get { return tester ?? (tester = HttpModuleTester.Create()); }
            set { tester = value; }
        }

        public void Dispose()
        {
            HttpModule = null;
        }

        [Fact]
        public void GetGlimpseRuntimeFromAppState()
        {
            var runtime = HttpModule.GetRuntime(HttpModule.AppStateMock.Object);

            Assert.Equal(HttpModule.GlimpseRuntimeWrapper, runtime);
        }

        [Fact]
        public void CallGlimpseRuntimeBeginRequestOnBeginRequest()
        {
            HttpModule.AppMock.Raise(m=> m.BeginRequest += null, EventArgs.Empty);
            HttpModule.RuntimeMock.Verify(r=>r.BeginRequest(), Times.Once());
        }

        [Fact]
        public void CallGlimpseRuntimeBeginRequestOnlyOnceEvenIfBeginRequestIsRaisedTwice()
        {
            HttpModule.AppMock.Raise(m => m.BeginRequest += null, EventArgs.Empty);
            HttpModule.AppMock.Raise(m => m.BeginRequest += null, EventArgs.Empty);
            HttpModule.RuntimeMock.Verify(r => r.BeginRequest(), Times.Once());
        }

        [Fact]
        public void CallGlimpseRuntimeBeginSessionAccessOnPostAcquireRequestState()
        {
            HttpModule.AppMock.Raise(m => m.PostAcquireRequestState += null, EventArgs.Empty);
            HttpModule.RuntimeMock.Verify(r => r.BeginSessionAccess(), Times.Once());
        }

        [Fact]
        public void CallGlimpseRuntimeBeginSessionAccessOnlyOnceEvenIfPostAcquireRequestStateIsRaisedTwice()
        {
            HttpModule.AppMock.Raise(m => m.PostAcquireRequestState += null, EventArgs.Empty);
            HttpModule.AppMock.Raise(m => m.PostAcquireRequestState += null, EventArgs.Empty);
            HttpModule.RuntimeMock.Verify(r => r.BeginSessionAccess(), Times.Once());
        }

        [Fact]
        public void CallGlimpseRuntimeBeginSessionAccessOnPostRequestHandlerExecute()
        {
            HttpModule.AppMock.Raise(m => m.PostRequestHandlerExecute += null, EventArgs.Empty);
            HttpModule.RuntimeMock.Verify(r => r.EndSessionAccess(), Times.Once());
        }

        [Fact]
        public void CallGlimpseRuntimeBeginSessionAccessOnlyOnceEvenIfPostRequestHandlerExecuteIsRaisedTwice()
        {
            HttpModule.AppMock.Raise(m => m.PostRequestHandlerExecute += null, EventArgs.Empty);
            HttpModule.AppMock.Raise(m => m.PostRequestHandlerExecute += null, EventArgs.Empty);
            HttpModule.RuntimeMock.Verify(r => r.EndSessionAccess(), Times.Once());
        }

        [Fact]
        public void CallGlimpseRuntimeBeginSessionAccessOnPostReleaseRequestState()
        {
            HttpModule.AppMock.Raise(m => m.PostReleaseRequestState += null, EventArgs.Empty);
            HttpModule.RuntimeMock.Verify(r => r.EndRequest(), Times.Once());
        }

        [Fact]
        public void CallGlimpseRuntimeBeginSessionAccessOnlyOnceEvenIfPostReleaseRequestStateIsRaisedTwice()
        {
            HttpModule.AppMock.Raise(m => m.PostReleaseRequestState += null, EventArgs.Empty);
            HttpModule.AppMock.Raise(m => m.PostReleaseRequestState += null, EventArgs.Empty);
            HttpModule.RuntimeMock.Verify(r => r.EndRequest(), Times.Once());
        }

        [Fact]
        public void DisposeNothing()
        {
            HttpModule.Dispose();
        }

        [Fact]
        public void LogAppDomainUnload()
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.SetData(Constants.LoggerKey, HttpModule.LoggerMock.Object);

            Glimpse.AspNet.HttpModule.OnDomainUnload(currentDomain);

            HttpModule.LoggerMock.Verify(l => l.Fatal(It.IsAny<string>(), It.IsAny<object[]>()));
        }

        [Fact]
        public void SetAppDomainLoggerOnInit()
        {
            HttpModule.Init(HttpModule.AppMock.Object);

            Assert.NotNull(AppDomain.CurrentDomain.GetData(Constants.LoggerKey));
        }
    }
}