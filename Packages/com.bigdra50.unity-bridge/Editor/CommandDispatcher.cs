using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityBridge.Helpers;

namespace UnityBridge
{
    /// <summary>
    /// Attribute to mark a class as a bridge tool handler
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BridgeToolAttribute : Attribute
    {
        public string CommandName { get; }

        public BridgeToolAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }

    /// <summary>
    /// Dispatches commands to registered handlers.
    /// Handlers are discovered automatically via the [BridgeTool] attribute.
    /// </summary>
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly Dictionary<string, Func<JObject, Task<JObject>>> _handlers;
        private bool _initialized;

        public CommandDispatcher()
        {
            _handlers = new Dictionary<string, Func<JObject, Task<JObject>>>();
        }

        /// <summary>
        /// Get all registered command names
        /// </summary>
        public IEnumerable<string> RegisteredCommands => _handlers.Keys;

        /// <summary>
        /// Initialize and discover all handlers
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
                return;

            DiscoverHandlers();
            _initialized = true;
        }

        /// <summary>
        /// Register a command handler manually
        /// </summary>
        public void Register(string commandName, Func<JObject, Task<JObject>> handler)
        {
            if (string.IsNullOrEmpty(commandName))
                throw new ArgumentNullException(nameof(commandName));

            _handlers[commandName] = handler ?? throw new ArgumentNullException(nameof(handler));
            BridgeLog.Verbose($"Registered handler: {commandName}");
        }

        /// <summary>
        /// Register a synchronous command handler
        /// </summary>
        public void Register(string commandName, Func<JObject, JObject> handler)
        {
            Register(commandName, p => Task.FromResult(handler(p)));
        }

        /// <summary>
        /// Unregister a command handler
        /// </summary>
        public void Unregister(string commandName)
        {
            _handlers.Remove(commandName);
        }

        /// <summary>
        /// Execute a command by name
        /// </summary>
        public async Task<JObject> ExecuteAsync(string commandName, JObject parameters)
        {
            if (!_initialized)
            {
                Initialize();
            }

            if (string.IsNullOrEmpty(commandName))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "Command name is required");
            }

            if (!_handlers.TryGetValue(commandName, out var handler))
            {
                throw new ProtocolException(
                    ErrorCode.CommandNotFound,
                    $"Unknown command: {commandName}");
            }

            try
            {
                var result = await handler(parameters ?? new JObject());
                return result ?? new JObject();
            }
            catch (ProtocolException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Unwrap TargetInvocationException to get the actual error
                var actualException = ex is TargetInvocationException tie && tie.InnerException != null
                    ? tie.InnerException
                    : ex;

                throw new ProtocolException(
                    ErrorCode.InternalError,
                    $"Command execution failed: {actualException.GetType().Name}: {actualException.Message}");
            }
        }

        private void DiscoverHandlers()
        {
            BridgeLog.Verbose("Discovering command handlers...");

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var handlerCount = 0;

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.GetCustomAttribute<BridgeToolAttribute>() != null);

                    foreach (var type in types)
                    {
                        var attr = type.GetCustomAttribute<BridgeToolAttribute>();
                        RegisterTypeHandler(type, attr.CommandName);
                        handlerCount++;
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    BridgeLog.Warn($"Failed to load types from assembly '{assembly.GetName().Name}': {ex.Message}");
                    if (ex.LoaderExceptions != null)
                    {
                        foreach (var loaderEx in ex.LoaderExceptions)
                        {
                            if (loaderEx != null)
                            {
                                BridgeLog.Warn($"  LoaderException: {loaderEx.Message}");
                            }
                        }
                    }
                }
            }

            BridgeLog.Verbose($"Discovered {handlerCount} command handlers");
        }

        private void RegisterTypeHandler(Type type, string commandName)
        {
            // Look for a static HandleCommand method
            var method = type.GetMethod(
                "HandleCommand",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(JObject) },
                null);

            if (method == null)
            {
                BridgeLog.Warn(
                    $"Handler {type.Name} missing HandleCommand(JObject) method");
                return;
            }

            // Check return type
            var returnType = method.ReturnType;

            if (returnType == typeof(Task<JObject>))
            {
                // Async handler
                _handlers[commandName] = parameters =>
                    (Task<JObject>)method.Invoke(null, new object[] { parameters });
            }
            else if (returnType == typeof(JObject))
            {
                // Sync handler
                _handlers[commandName] = parameters =>
                    Task.FromResult((JObject)method.Invoke(null, new object[] { parameters }));
            }
            else if (returnType == typeof(object))
            {
                // Generic object handler - convert to JObject
                _handlers[commandName] = parameters =>
                {
                    var result = method.Invoke(null, new object[] { parameters });
                    if (result is JObject jobj)
                        return Task.FromResult(jobj);
                    return Task.FromResult(JObject.FromObject(result ?? new object()));
                };
            }
            else
            {
                BridgeLog.Warn(
                    $"Handler {type.Name}.HandleCommand has unsupported return type: {returnType}");
                return;
            }

            BridgeLog.Verbose($"Registered handler: {commandName} ({type.Name})");
        }
    }
}
