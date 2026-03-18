using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMRazorImproved.Core.Proto;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services
{
    /// <summary>
    /// Server WebSocket che espone le funzionalità di scripting e registrazione
    /// di TMRazorImproved a tool esterni tramite protobuf over WebSocket.
    /// Porta di ascolto: prima porta libera nell'intervallo 15454-15463.
    /// Endpoint: ws://127.0.0.1:{port}/proto
    /// </summary>
    public sealed class ProtoControlService : IProtoControlService, IDisposable
    {
        private readonly IScriptingService _scriptingService;
        private readonly IScriptRecorderService _scriptRecorderService;
        private readonly ILogger<ProtoControlService> _logger;

        private HttpListener? _httpListener;
        private CancellationTokenSource? _serverCts;
        private Task? _acceptLoopTask;

        // sessionId -> CancellationTokenSource per stop delle sessioni attive
        private readonly ConcurrentDictionary<int, CancellationTokenSource> _activeSessions = new();

        private const int StartPort = 15454;
        private const int PortRange = 10;
        private const long TimeGateMs = 100;

        public int? Port { get; private set; }
        public bool IsRunning => _httpListener?.IsListening == true;

        public ProtoControlService(
            IScriptingService scriptingService,
            IScriptRecorderService scriptRecorderService,
            ILogger<ProtoControlService> logger)
        {
            _scriptingService = scriptingService;
            _scriptRecorderService = scriptRecorderService;
            _logger = logger;
        }

        public bool Start()
        {
            if (IsRunning) return true;

            int port = FindAvailablePort(StartPort, PortRange);
            if (port < 0)
            {
                _logger.LogError("[ProtoControl] Nessuna porta disponibile nell'intervallo {Start}-{End}", StartPort, StartPort + PortRange - 1);
                return false;
            }

            try
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add($"http://127.0.0.1:{port}/proto/");
                _httpListener.Start();

                Port = port;
                _serverCts = new CancellationTokenSource();
                _acceptLoopTask = Task.Run(() => AcceptLoopAsync(_serverCts.Token));

                _logger.LogInformation("[ProtoControl] Server WebSocket avviato su ws://127.0.0.1:{Port}/proto", port);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProtoControl] Errore avvio server");
                _httpListener = null;
                Port = null;
                return false;
            }
        }

        public void Stop()
        {
            _serverCts?.Cancel();
            foreach (var cts in _activeSessions.Values)
                cts.Cancel();
            _activeSessions.Clear();

            try { _httpListener?.Stop(); } catch { }
            _httpListener = null;
            Port = null;
            _logger.LogInformation("[ProtoControl] Server WebSocket fermato");
        }

        private async Task AcceptLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _httpListener?.IsListening == true)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        _ = HandleClientAsync(context, ct);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
                catch (HttpListenerException) when (ct.IsCancellationRequested) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[ProtoControl] Errore accettazione connessione");
                }
            }
        }

        private async Task HandleClientAsync(HttpListenerContext httpContext, CancellationToken serverCt)
        {
            WebSocket? ws = null;
            try
            {
                var wsContext = await httpContext.AcceptWebSocketAsync(null);
                ws = wsContext.WebSocket;
                _logger.LogDebug("[ProtoControl] Client connesso");

                var sendLock = new SemaphoreSlim(1, 1);
                var buffer = new byte[256 * 1024];

                while (ws.State == WebSocketState.Open && !serverCt.IsCancellationRequested)
                {
                    WebSocketReceiveResult result;
                    try
                    {
                        result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), serverCt);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (WebSocketException) { break; }

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", serverCt);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Binary && result.Count > 0)
                    {
                        var data = buffer[..result.Count];
                        _ = HandleMessageAsync(ws, sendLock, data, serverCt);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ProtoControl] Errore client WebSocket");
            }
            finally
            {
                ws?.Dispose();
                _logger.LogDebug("[ProtoControl] Client disconnesso");
            }
        }

        private async Task HandleMessageAsync(WebSocket ws, SemaphoreSlim sendLock, byte[] data, CancellationToken ct)
        {
            var (msgType, sessionId) = ReadTypeAndSession(data);
            if (msgType == ProtoMessageType.UndefinedType || sessionId == 0)
            {
                _logger.LogWarning("[ProtoControl] Messaggio non valido: type={Type} session={Session}", msgType, sessionId);
                return;
            }

            switch (msgType)
            {
                case ProtoMessageType.PlayRequestType:
                    var playReq = PlayRequest.Parser.ParseFrom(data);
                    await HandlePlayAsync(ws, sendLock, playReq, ct);
                    break;

                case ProtoMessageType.StopPlayRequestType:
                    var stopPlayReq = StopPlayRequest.Parser.ParseFrom(data);
                    HandleStopPlay(ws, sendLock, stopPlayReq, ct);
                    break;

                case ProtoMessageType.RecordRequestType:
                    var recReq = RecordRequest.Parser.ParseFrom(data);
                    await HandleRecordAsync(ws, sendLock, recReq, ct);
                    break;

                case ProtoMessageType.StopRecordRequestType:
                    var stopRecReq = StopRecordRequest.Parser.ParseFrom(data);
                    await HandleStopRecordAsync(ws, sendLock, stopRecReq, ct);
                    break;

                default:
                    _logger.LogDebug("[ProtoControl] Tipo messaggio non gestito: {Type}", msgType);
                    break;
            }
        }

        private async Task HandlePlayAsync(WebSocket ws, SemaphoreSlim sendLock, PlayRequest request, CancellationToken serverCt)
        {
            var sessionCts = new CancellationTokenSource();
            _activeSessions[request.Sessionid] = sessionCts;
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(serverCt, sessionCts.Token);

            var language = MapLanguage(request.Language);
            var code = string.Join("\n", request.Commands);
            var stopwatch = Stopwatch.StartNew();
            long lastSendMs = 0;

            void OnOutput(string line)
            {
                // Rispetta il time gate (100ms) per evitare overflow
                long now = stopwatch.ElapsedMilliseconds;
                if (now - lastSendMs < TimeGateMs)
                    Thread.Sleep((int)(TimeGateMs - (now - lastSendMs)));
                lastSendMs = stopwatch.ElapsedMilliseconds;

                _ = SendPlayResponseAsync(ws, sendLock, request.Sessionid, line, more: true, serverCt);
            }

            _scriptingService.OutputReceived += OnOutput;
            _scriptingService.ErrorReceived += OnOutput;
            try
            {
                await _scriptingService.RunAsync(code, language, $"proto_{request.Sessionid}", linkedCts.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ProtoControl] Errore esecuzione script");
            }
            finally
            {
                _scriptingService.OutputReceived -= OnOutput;
                _scriptingService.ErrorReceived -= OnOutput;
                _activeSessions.TryRemove(request.Sessionid, out _);
            }

            await SendPlayResponseAsync(ws, sendLock, request.Sessionid, "finished", more: false, serverCt);
        }

        private void HandleStopPlay(WebSocket ws, SemaphoreSlim sendLock, StopPlayRequest request, CancellationToken ct)
        {
            if (_activeSessions.TryGetValue(request.Sessionid, out var cts))
            {
                cts.Cancel();
                _ = _scriptingService.StopAsync();
            }

            var response = new StopPlayResponse
            {
                Type = ProtoMessageType.StopPlayResponseType,
                Sessionid = request.Sessionid,
                Success = true
            };
            _ = SendRawAsync(ws, sendLock, response.ToByteArray(), ct);
        }

        private async Task HandleRecordAsync(WebSocket ws, SemaphoreSlim sendLock, RecordRequest request, CancellationToken serverCt)
        {
            var language = MapLanguage(request.Language);
            if (language == ScriptLanguage.CSharp)
            {
                // La registrazione non è supportata per C#
                await SendRecordResponseAsync(ws, sendLock, request.Sessionid, "C# recording not supported", more: false, serverCt);
                return;
            }

            var sessionCts = new CancellationTokenSource();
            _activeSessions[request.Sessionid] = sessionCts;
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(serverCt, sessionCts.Token);

            _scriptRecorderService.StartRecording(language);
            _logger.LogDebug("[ProtoControl] Registrazione avviata in {Language}", language);

            string lastSentScript = "";
            var stopwatch = Stopwatch.StartNew();

            try
            {
                while (!linkedCts.Token.IsCancellationRequested)
                {
                    await Task.Delay(100, linkedCts.Token);

                    var currentScript = _scriptRecorderService.GetRecordedScript() ?? "";
                    if (currentScript.Length > lastSentScript.Length)
                    {
                        var newPart = currentScript[lastSentScript.Length..];
                        var lines = newPart.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            // Time gate
                            long elapsed = stopwatch.ElapsedMilliseconds;
                            if (elapsed < TimeGateMs)
                                await Task.Delay((int)(TimeGateMs - elapsed), linkedCts.Token);
                            stopwatch.Restart();

                            await SendRecordResponseAsync(ws, sendLock, request.Sessionid, line.TrimEnd('\r'), more: true, serverCt);
                        }
                        lastSentScript = currentScript;
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _scriptRecorderService.StopRecording();
                _activeSessions.TryRemove(request.Sessionid, out _);
                _logger.LogDebug("[ProtoControl] Registrazione fermata");
            }

            await SendRecordResponseAsync(ws, sendLock, request.Sessionid, "Recording Stopped", more: false, serverCt);
        }

        private async Task HandleStopRecordAsync(WebSocket ws, SemaphoreSlim sendLock, StopRecordRequest request, CancellationToken ct)
        {
            if (_activeSessions.TryGetValue(request.Sessionid, out var cts))
                cts.Cancel();

            var response = new StopRecordResponse
            {
                Type = ProtoMessageType.StopRecordResponseType,
                Sessionid = request.Sessionid,
                Success = true
            };
            await SendRawAsync(ws, sendLock, response.ToByteArray(), ct);
        }

        private async Task SendPlayResponseAsync(WebSocket ws, SemaphoreSlim sendLock, int sessionId, string result, bool more, CancellationToken ct)
        {
            var response = new PlayResponse
            {
                Type = ProtoMessageType.PlayResponseType,
                Sessionid = sessionId,
                More = more,
                Result = result
            };
            await SendRawAsync(ws, sendLock, response.ToByteArray(), ct);
        }

        private async Task SendRecordResponseAsync(WebSocket ws, SemaphoreSlim sendLock, int sessionId, string data, bool more, CancellationToken ct)
        {
            var response = new RecordResponse
            {
                Type = ProtoMessageType.RecordResponseType,
                Sessionid = sessionId,
                More = more,
                Data = data
            };
            await SendRawAsync(ws, sendLock, response.ToByteArray(), ct);
        }

        private static async Task SendRawAsync(WebSocket ws, SemaphoreSlim sendLock, byte[] data, CancellationToken ct)
        {
            if (ws.State != WebSocketState.Open) return;
            await sendLock.WaitAsync(ct);
            try
            {
                await ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, ct);
            }
            catch (OperationCanceledException) { }
            catch (WebSocketException) { }
            finally
            {
                sendLock.Release();
            }
        }

        private static (ProtoMessageType type, int sessionId) ReadTypeAndSession(byte[] buffer)
        {
            using var input = new CodedInputStream(buffer);
            ProtoMessageType messageType = ProtoMessageType.UndefinedType;
            int sessionId = 0;

            uint tag = input.ReadTag();
            if (tag > 0 && WireFormat.GetTagFieldNumber(tag) == 1)
                messageType = (ProtoMessageType)input.ReadEnum();

            tag = input.ReadTag();
            if (tag > 0 && WireFormat.GetTagFieldNumber(tag) == 2)
                sessionId = input.ReadInt32();

            return (messageType, sessionId);
        }

        private static ScriptLanguage MapLanguage(ProtoLanguage lang) => lang switch
        {
            ProtoLanguage.Python => ScriptLanguage.Python,
            ProtoLanguage.Csharp => ScriptLanguage.CSharp,
            ProtoLanguage.Uosteam => ScriptLanguage.UOSteam,
            _ => ScriptLanguage.Python
        };

        private static int FindAvailablePort(int startPort, int range)
        {
            for (int port = startPort; port < startPort + range; port++)
            {
                TcpListener? listener = null;
                try
                {
                    listener = new TcpListener(IPAddress.Loopback, port);
                    listener.Start();
                    return port;
                }
                catch (SocketException) { }
                finally { listener?.Stop(); }
            }
            return -1;
        }

        public void Dispose() => Stop();
    }
}
