using Common.Application.Common;
using Common.Application.Contracts.interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.infrastructure.Services.EventPublisher
{
    public class EventService : IEventService, IDisposable
    {
        private readonly ConcurrentDictionary<string, List<Func<EventMessage, Task>>> _subscribers;
        private readonly BlockingCollection<EventMessage> _syncEventQueue;
        private readonly BlockingCollection<EventMessage> _asyncEventQueue;
        private readonly IAppLogger<EventService> _logger;
        private readonly CancellationTokenSource _cts;
        private readonly Task[] _processingTasks;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public EventService(IAppLogger<EventService> logger,
                          int maxQueueSize = 1000,
                          int maxDegreeOfParallelism = 5)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subscribers = new ConcurrentDictionary<string, List<Func<EventMessage, Task>>>();
            _syncEventQueue = new BlockingCollection<EventMessage>(maxQueueSize);
            _asyncEventQueue = new BlockingCollection<EventMessage>(maxQueueSize);
            _cts = new CancellationTokenSource();
            _semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);

            // بدء معالجة الأحداث المتزامنة وغير المتزامنة في الخلفية
            _processingTasks = new[]
            {
                Task.Run(() => ProcessSyncEventsAsync(), _cts.Token),
                Task.Run(() => ProcessAsyncEventsAsync(), _cts.Token)
            };
        }

        public bool Publish(string eventType, object payload)
        {
            ValidateEventParameters(eventType, payload);

            try
            {
                var eventMessage = CreateEventMessage(eventType, payload);
                return _syncEventQueue.TryAdd(eventMessage, 100, _cts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish sync event {EventType}", eventType);
                return false;
            }
        }

        public async Task<bool> PublishAsync(string eventType, object payload)
        {
            ValidateEventParameters(eventType, payload);

            try
            {
                var eventMessage = CreateEventMessage(eventType, payload);
                return await Task.Run(() => _asyncEventQueue.TryAdd(eventMessage, 100, _cts.Token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish async event {EventType}", eventType);
                return false;
            }
        }

        public bool Subscribe(string eventType, Func<EventMessage, Task> handler)
        {
            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Event type cannot be null or empty", nameof(eventType));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            try
            {
                var handlers = _subscribers.GetOrAdd(eventType, _ => new List<Func<EventMessage, Task>>());

                lock (handlers)
                {
                    if (!handlers.Contains(handler))
                    {
                        handlers.Add(handler);
                        return true;
                    }

                    _logger.LogWarning("Handler already subscribed for event {EventType}", eventType);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to subscribe to event {EventType}", eventType);
                return false;
            }
        }

        public bool Unsubscribe(string eventType, Func<EventMessage, Task> handler)
        {
            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Event type cannot be null or empty", nameof(eventType));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            try
            {
                if (_subscribers.TryGetValue(eventType, out var handlers))
                {
                    lock (handlers)
                    {
                        return handlers.Remove(handler);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unsubscribe from event {EventType}", eventType);
                return false;
            }
        }

        private async Task ProcessSyncEventsAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var eventMessage = _syncEventQueue.Take(_cts.Token);
                    await ProcessEventMessage(eventMessage, isAsync: false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in sync event processing loop");
                }
            }
        }

        private async Task ProcessAsyncEventsAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var eventMessage = _asyncEventQueue.Take(_cts.Token);
                    await ProcessEventMessage(eventMessage, isAsync: true);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in async event processing loop");
                }
            }
        }

        private async Task ProcessEventMessage(EventMessage eventMessage, bool isAsync)
        {
            if (_subscribers.TryGetValue(eventMessage.Type, out var handlers))
            {
                var handlerTasks = handlers.Select(handler =>
                    ExecuteHandler(handler, eventMessage, isAsync)).ToList();

                await Task.WhenAll(handlerTasks);
            }
        }

        private async Task ExecuteHandler(Func<EventMessage, Task> handler,
                                       EventMessage eventMessage,
                                       bool isAsync)
        {
            await _semaphore.WaitAsync(_cts.Token);

            try
            {
                var task = handler(eventMessage);

                if (!isAsync)
                {
                    await task.ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Event handling was canceled for {EventType}", eventMessage.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in event handler for {EventType}", eventMessage.Type);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void ValidateEventParameters(string eventType, object payload)
        {
            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Event type cannot be null or empty", nameof(eventType));

            if (payload == null)
                throw new ArgumentNullException(nameof(payload));
        }

        private EventMessage CreateEventMessage(string eventType, object payload)
        {
            return new EventMessage
            {
                Type = eventType,
                Payload = payload,
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    { "source", "3lamny" },
                    { "version", "1.0" },
                    { "environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production" }
                }
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                try
                {
                    _cts.Cancel();

                    Task.WaitAll(_processingTasks, TimeSpan.FromSeconds(5));

                    _syncEventQueue?.Dispose();
                    _asyncEventQueue?.Dispose();
                    _semaphore?.Dispose();
                    _cts?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during disposal");
                }
            }

            _disposed = true;
        }

        ~EventService()
        {
            Dispose(false);
        }
    }

}
