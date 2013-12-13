using System;
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

        public static AppFunc Auth(Func<Environment, Task<bool>> userValidator, AppFunc onAuthenticated)
        {
            return async env =>
            {
                if (await userValidator(env))
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

        public static MiddlewareFunc Auth(Func<Environment, Task<bool>> userValidator)
        {
            return async (env, next) =>
            {
                if (await userValidator(env))
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
}