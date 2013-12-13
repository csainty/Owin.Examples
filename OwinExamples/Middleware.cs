using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using Environment = System.Collections.Generic.IDictionary<string, object>;
using MiddlewareFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>, System.Threading.Tasks.Task>;

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

    // Middleware based solely on AppFunc
    public static class AppFuncs
    {
        public static AppFunc Pipeline(params AppFunc[] middleware)
        {
            return async env =>
            {
                foreach (var item in middleware)
                {
                    await item(env);
                }
            };
        }

        public static AppFunc Auth(IUserValidator userValidator, AppFunc onAuthenticated)
        {
            return async env =>
            {
                if (await userValidator.IsLoggedIn(env))
                {
                    await onAuthenticated(env);
                    return;
                }
                env["owin.ResponseStatusCode"] = 401;
            };
        }

        public static AppFunc View(string viewName)
        {
            return env =>
            {
                var body = Encoding.UTF8.GetBytes(viewName);
                return ((Stream)env["owin.ResponseBody"]).WriteAsync(body, 0, body.Length);
            };
        }

        public static Task LogResponseCode(Environment env)
        {
            Debug.WriteLine(env["owin.ResponseStatusCode"]);
            return Task.FromResult(0);
        }
    }

    // Middleware based on a common "next passing" middleware signature
    public static class MiddlewareFuncs
    {
        public static AppFunc Pipeline(params MiddlewareFunc[] middleware)
        {
            AppFunc next = env => Task.FromResult(0);
            foreach (var item in middleware.Reverse())
            {
                var onNext = next;
                next = env => item(env, onNext);
            }
            return next;
        }

        public static MiddlewareFunc Auth(IUserValidator validator)
        {
            return async (env, next) =>
            {
                if (await validator.IsLoggedIn(env))
                {
                    await next(env);
                    return;
                }
                env["owin.ResponseStatusCode"] = 401;
            };
        }

        public static MiddlewareFunc View(string viewName)
        {
            return async (env, next) =>
            {
                var body = Encoding.UTF8.GetBytes(viewName);
                await ((Stream)env["owin.ResponseBody"]).WriteAsync(body, 0, body.Length);
                await next(env);
            };
        }

        public static async Task LogResponseCode(Environment env, AppFunc next)
        {
            await next(env);
            Debug.WriteLine(env["owin.ResponseStatusCode"]);
        }
    }

    public interface IUserValidator
    {
        Task<bool> IsLoggedIn(Environment environment);
    }

    public class RouteUserValidator : IUserValidator
    {
        public Task<bool> IsLoggedIn(Environment environment)
        {
            return Task.FromResult(environment["owin.RequestPath"].ToString() == "/auth");
        }
    }
}