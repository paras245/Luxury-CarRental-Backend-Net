using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace CarRental_Backend_Net.Middleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public TokenBlacklistMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                if (_cache.TryGetValue($"blacklist:{token}", out _))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Token has been logged out.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
