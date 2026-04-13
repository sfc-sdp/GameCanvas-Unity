using Newtonsoft.Json.Linq;

namespace UnityBridge.Helpers
{
    /// <summary>
    /// Interface for all bridge response types.
    /// </summary>
    public interface IBridgeResponse
    {
        bool Success { get; }
        JObject ToJson();
    }

    /// <summary>
    /// Represents a successful response.
    /// </summary>
    public sealed class SuccessResponse : IBridgeResponse
    {
        public bool Success => true;
        public string Message { get; }
        public object Data { get; }

        public SuccessResponse(string message, object data = null)
        {
            Message = message;
            Data = data;
        }

        public JObject ToJson()
        {
            var json = new JObject
            {
                ["success"] = true,
                ["message"] = Message
            };

            if (Data != null)
            {
                json["data"] = Data is JToken token ? token : JToken.FromObject(Data);
            }

            return json;
        }
    }

    /// <summary>
    /// Represents an error response.
    /// </summary>
    public sealed class ErrorResponse : IBridgeResponse
    {
        public bool Success => false;
        public string Code { get; }
        public string Error { get; }
        public object Data { get; }

        public ErrorResponse(string code, string error, object data = null)
        {
            Code = code;
            Error = error;
            Data = data;
        }

        public JObject ToJson()
        {
            var json = new JObject
            {
                ["success"] = false,
                ["code"] = Code,
                ["error"] = Error
            };

            if (Data != null)
            {
                json["data"] = Data is JToken token ? token : JToken.FromObject(Data);
            }

            return json;
        }
    }

    /// <summary>
    /// Represents a pending response for long-running operations.
    /// </summary>
    public sealed class PendingResponse : IBridgeResponse
    {
        public bool Success => true;
        public string Status => "pending";
        public double PollIntervalSeconds { get; }
        public string Hint { get; }

        public PendingResponse(double pollIntervalSeconds, string hint = null)
        {
            PollIntervalSeconds = pollIntervalSeconds;
            Hint = hint;
        }

        public JObject ToJson()
        {
            var json = new JObject
            {
                ["success"] = true,
                ["status"] = Status,
                ["poll_interval_seconds"] = PollIntervalSeconds
            };

            if (Hint != null)
            {
                json["hint"] = Hint;
            }

            return json;
        }
    }
}
