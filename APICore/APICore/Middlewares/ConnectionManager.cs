using APICore.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace APICore.Middlewares
{
    public class ConnectionManager
    {
        private ConcurrentDictionary<string, WebSocketMap> _sockets = new ConcurrentDictionary<string, WebSocketMap>();

        public WebSocketMap GetSocketById(string id)
        {
            return _sockets.FirstOrDefault(p => p.Key == id).Value;
        }

        public ConcurrentDictionary<string, WebSocketMap> GetAll()
        {
            return _sockets;
        }

        public string GetId(WebSocketMap socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }
        public void AddSocket(WebSocketMap socket)
        {
            _sockets.TryAdd(CreateConnectionId(), socket);
        }

        public async Task RemoveSocket(string id)
        {
            WebSocketMap socket;
            _sockets.TryRemove(id, out socket);

            await socket.WebSocket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                    statusDescription: "Closed by the ConnectionManager",
                                    cancellationToken: CancellationToken.None);
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
