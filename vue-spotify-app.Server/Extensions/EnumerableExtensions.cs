using System.Collections.Generic;
using System.Linq;

namespace vue_spotify_app.Server.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) =>
            source == null || !source.Any();
    }
}