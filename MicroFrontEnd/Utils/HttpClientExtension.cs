using System.Net.Http.Headers;

namespace MicroFrontEnd.Utils
{
    public static class HttpClientExtensions
    {
        public static void AddJwtFromSession(this HttpClient client, IHttpContextAccessor accessor)
        {
            Console.WriteLine("🔥 AddJwtFromSession exécutée !");
            var token = accessor.HttpContext?.Session.GetString("JwtToken");
            Console.WriteLine(">>> TOKEN : " + (token ?? "NULL"));

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            Console.WriteLine("TOKEN (AddJwtFromSession): " + token);
        }
    }
}
