using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UnityBridge.Helpers
{
    internal static class PortManager
    {
        private const int PollingIntervalMs = 100;

        public static bool IsPortAvailable(int port)
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
            finally
            {
                listener?.Stop();
            }
        }

        public static bool WaitForPortRelease(int port, int timeoutMs = 5000)
        {
            if (timeoutMs < 0)
                throw new ArgumentOutOfRangeException(nameof(timeoutMs), "Timeout must be non-negative");

            var elapsed = 0;

            while (elapsed < timeoutMs)
            {
                if (IsPortAvailable(port))
                {
                    BridgeLog.Verbose($"Port {port} is now available (waited {elapsed}ms)");
                    return true;
                }

                Thread.Sleep(PollingIntervalMs);
                elapsed += PollingIntervalMs;
            }

            BridgeLog.Warn($"Port {port} did not become available within {timeoutMs}ms");
            return false;
        }
    }
}
