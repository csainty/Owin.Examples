using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace OwinExamples
{
    public static class Server
    {
        public static AppFunc AppFuncServer()
        {
            // AppFunc all the way down
            // Each middleware can provide a builder if it needs parameters
            // Each middleware explcitly requires AppFuncs if it is going to branch
            // The entire pipeline is run on each request, following appropriate branches
            return AppFuncs.Pipeline(
                AppFuncs.Auth(new RouteUserValidator(),
                    AppFuncs.View("Hello World")
                ),
                AppFuncs.LogResponseCode
            );
        }

        public static AppFunc MiddlewareFuncServer()
        {
            // New MiddlewareFunc specification
            // Linear pipeline without branching
            // Any middleware is free to stop processing
            // Any "finally" type middleware is declared first so it wraps the rest of the pipeline
            return MiddlewareFuncs.Pipeline(
                MiddlewareFuncs.LogResponseCode,
                MiddlewareFuncs.Auth(new RouteUserValidator()),
                MiddlewareFuncs.View("Hello World")
            );
        }
    }
}