using DSerfozo.CefGlue.Contract.Renderer;

namespace DSerfozo.RpcBindings.CefGlue.Renderer.Services
{
    public class PromiseUserData : ICefUserData
    {
        public ICefV8Value PromiseCreator { get; }

        public ICefV8Value IsPromise { get; }

        public ICefV8Value WaitForPromise { get; }

        public PromiseUserData(ICefV8Value promiseCreator, ICefV8Value isPromise, ICefV8Value waitForPromise)
        {
            this.PromiseCreator = promiseCreator;
            this.IsPromise = isPromise;
            this.WaitForPromise = waitForPromise;
        }


        public void Dispose()
        {
            PromiseCreator.Dispose();
            IsPromise.Dispose();
            WaitForPromise.Dispose();
        }
    }
}
