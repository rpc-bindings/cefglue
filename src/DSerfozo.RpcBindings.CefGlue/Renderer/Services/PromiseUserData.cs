using Xilium.CefGlue;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Services
{
    public class PromiseUserData : CefUserData
    {
        public CefV8Value PromiseCreator { get; }

        public CefV8Value IsPromise { get; }

        public CefV8Value WaitForPromise { get; }

        public PromiseUserData(CefV8Value promiseCreator, CefV8Value isPromise, CefV8Value waitForPromise)
        {
            this.PromiseCreator = promiseCreator;
            this.IsPromise = isPromise;
            this.WaitForPromise = waitForPromise;
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                PromiseCreator.Dispose();
                IsPromise.Dispose();
                WaitForPromise.Dispose();
            }
        }
    }
}
