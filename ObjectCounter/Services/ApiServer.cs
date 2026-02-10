using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ObjectCounter.Services;

namespace ObjectCounter.Services
{
    public class ApiServer
    {
        private HttpListener? _listener;
        private readonly DatabaseService _dbService;
        private bool _isRunning = false;
        private Thread? _serverThread;

        public ApiServer(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public void Start(int port = 8000)
        {
            if (_isRunning) return;

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://*:{port}/");
                _listener.Start();
                _isRunning = true;
                _serverThread = new Thread(Listen);
                _serverThread.Start();
                Console.WriteLine($"API Server started on port {port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Server Start Error: {ex.Message}");
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
        }

        private void Listen()
        {
            while (_isRunning && _listener != null && _listener.IsListening)
            {
                try
                {
                    var context = _listener.GetContext();
                    Task.Run(() => HandleRequest(context));
                }
                catch (HttpListenerException)
                {
                    // Listener stopped
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"API Listen Error: {ex.Message}");
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                var response = context.Response;
                var rawUrl = context.Request.RawUrl;

                string responseString = "{\"status\": \"ok\"}";

                if (rawUrl != null && rawUrl.Contains("/api/logs"))
                {
                    var logs = _dbService.GetLogs(50);
                    // Simple JSON serialization (manual to avoid NewtonSoft dep)
                    var sb = new StringBuilder();
                    sb.Append("[");
                    for (int i = 0; i < logs.Count; i++)
                    {
                        var log = logs[i];
                        sb.Append($"{{\"id\":{log.Id},\"time\":\"{log.Timestamp:O}\",\"obj\":\"{log.ObjectType}\",\"count\":{log.Count}}}");
                        if (i < logs.Count - 1) sb.Append(",");
                    }
                    sb.Append("]");
                    responseString = sb.ToString();
                    response.ContentType = "application/json";
                }
                else
                {
                    response.ContentType = "application/json";
                }

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Handle Error: {ex.Message}");
            }
        }
    }
}
