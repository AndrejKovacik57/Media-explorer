namespace WpfApp1.Models;
using System.Net;
using System.IO;

public class MediaServer
{
    private HttpListener _listener;
    private bool _isRunning;

    public void StartServer(string videoFilePath, string port)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://+:{port}/");
        _listener.Start();
        _isRunning = true;
        Console.WriteLine($"Server started on http://localhost:{port}/ ...");

        // Start handling requests in a separate task
        Task.Run(() => HandleRequests(videoFilePath));
    }

    private async Task HandleRequests(string videoFilePath)
    {
        while (_isRunning)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                var request = context.Request;
                var response = context.Response;

                long fileLength = new FileInfo(videoFilePath).Length;
                string rangeHeader = request.Headers["Range"];

                if (!string.IsNullOrEmpty(rangeHeader))
                {
                    // Handle range request
                    string[] range = rangeHeader.Replace("bytes=", "").Split('-');
                    long start = long.Parse(range[0]);
                    long end = range.Length > 1 && !string.IsNullOrEmpty(range[1]) ? long.Parse(range[1]) : fileLength - 1;

                    if (start >= 0 && start < fileLength)
                    {
                        response.StatusCode = (int)HttpStatusCode.PartialContent;
                        response.AddHeader("Content-Range", $"bytes {start}-{end}/{fileLength}");
                        response.ContentLength64 = (end - start) + 1;

                        byte[] buffer = new byte[end - start + 1];
                        using (FileStream fs = new FileStream(videoFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            fs.Seek(start, SeekOrigin.Begin);
                            await fs.ReadAsync(buffer, 0, buffer.Length);
                        }

                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable;
                        response.AddHeader("Content-Range", $"bytes */{fileLength}");
                    }
                }
                else
                {
                    // Handle full file request
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.ContentType = "video/mp4";
                    response.ContentLength64 = fileLength;

                    using (FileStream fs = new FileStream(videoFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        byte[] buffer = new byte[64 * 1024]; // 64KB buffer
                        int bytesRead;
                        while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await response.OutputStream.WriteAsync(buffer, 0, bytesRead);
                        }
                    }
                }

                response.OutputStream.Close();
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 995)
            {
                // Ignore the I/O aborted error as this is expected when clients cancel the request
                Console.WriteLine("Client cancelled the request (HTTP 995).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while handling request: {ex.Message}");
            }
        }
    }


    public void StopServer()
    {
        _isRunning = false;
        _listener.Stop();
        Console.WriteLine("Server stopped.");
    }
}
