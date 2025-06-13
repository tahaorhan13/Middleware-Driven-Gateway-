namespace MiddlewareCollection.Gateway.API.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly Dictionary<string, (int Count, DateTime ResetTime)> _ipCache = new();
        private readonly int _limit = 100;
        private readonly TimeSpan _window = TimeSpan.FromMinutes(1);

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            if (ip is null)
            {
                await _next(context);
                return;
            }

            lock (_ipCache)
            {
                if (_ipCache.ContainsKey(ip))
                {
                    var (count, reset) = _ipCache[ip];
                    if (reset > DateTime.UtcNow)
                    {
                        if (count >= _limit)
                        {
                            context.Response.StatusCode = 429;
                            context.Response.Headers["Retry-After"] = (reset - DateTime.UtcNow).TotalSeconds.ToString("F0");
                            return;
                        }

                        _ipCache[ip] = (count + 1, reset);
                    }
                    else
                    {
                        _ipCache[ip] = (1, DateTime.UtcNow.Add(_window));
                    }
                }
                else
                {
                    _ipCache[ip] = (1, DateTime.UtcNow.Add(_window));
                }
            }

            await _next(context);
        }
    }
}
