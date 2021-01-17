using APICore.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace APICore.Middlewares
{
    public class WebSocketManagerMiddleware
    {
        private readonly RequestDelegate _next;
        private WebSocketHandler _webSocketHandler { get; set; }

        public WebSocketManagerMiddleware(RequestDelegate next,
                                            WebSocketHandler webSocketHandler)
        {
            _next = next;
            _webSocketHandler = webSocketHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            var token = context.Request.Query["token"].ToString();

            if (!CoreHelper.ValidateCurrentToken(token))
            {
                socket.Abort();
                Console.WriteLine("Invalid token");
                return;
            }

            // Get user claims
            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(token) as JwtSecurityToken;
            var userIDstr = tokenS.Claims.FirstOrDefault(claim => claim.Type == "nameid")?.Value ?? tokenS.Claims.FirstOrDefault(claim => claim.Type == "unique_name")?.Value;
            int.TryParse(userIDstr, out int userId);

            await _webSocketHandler.OnConnected(new Models.WebSocketMap
            {
                WebSocket = socket,
                UserId = userId,
            });

            await Receive(socket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {

                    await _webSocketHandler.ReceiveAsync(new Models.WebSocketMap
                    {
                        UserId = userId,
                        WebSocket = socket,
                    }, result, buffer);
                    return;
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocketHandler.OnDisconnected(new Models.WebSocketMap
                    {
                        WebSocket = socket,
                    });
                    return;
                }

            });
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                                        cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }
}
