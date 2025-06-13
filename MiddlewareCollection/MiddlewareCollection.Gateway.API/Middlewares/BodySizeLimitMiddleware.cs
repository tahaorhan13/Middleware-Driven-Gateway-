namespace MiddlewareCollection.Gateway.API.Middlewares
{
    public class BodySizeLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly long _maxSizeInBytes;

        public BodySizeLimitMiddleware(RequestDelegate next, long maxSizeInMb = 1)
        {
            _next = next;
            _maxSizeInBytes = maxSizeInMb * 1024 * 1024;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();

            if (context.Request.ContentLength > _maxSizeInBytes)
            {
                context.Response.StatusCode = 413;
                await context.Response.WriteAsync("İçerik boyutu çok büyük.");
                return;
            }

            await _next(context);
        }
    }
}
