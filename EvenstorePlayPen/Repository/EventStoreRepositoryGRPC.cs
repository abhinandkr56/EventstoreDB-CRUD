using System.Text;
using EvenstorePlayPen.Domain;
using EvenstorePlayPen.Models;
using EventStore.Client;
using Newtonsoft.Json;

namespace EvenstorePlayPen.Repository;

public class EventStoreRepositoryGRPC
{
    private readonly EventStoreClient _client;
    private readonly IConfiguration _configuration;

    public EventStoreRepositoryGRPC(IConfiguration configuration)
    {
        _configuration = configuration;
        var settings = EventStoreClientSettings.Create(_configuration["eventStoreGrpc:connectionString"]);
        _client = new EventStoreClient(settings);
    }

    public async Task AppendEvents(string streamName, object data)
    {
        var eventData = new EventData(
            Uuid.NewUuid(), 
            data.GetType().AssemblyQualifiedName, 
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)), 
            metadata: null);

        await _client.AppendToStreamAsync(
            streamName, 
            StreamState.Any, 
            new[] { eventData });
    }

    public async Task<T> LoadAsync<T>(string streamName) where T : IAggregate, new()
    {
        var aggregate = new T();
        var events = _client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start);

        await foreach (var resolvedEvent in events)
        {
            var eventType = Type.GetType(resolvedEvent.Event.EventType);
            var eventData = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span), eventType);

            aggregate.ApplyEvent(eventData);
        }

        return aggregate;
    }

    public async Task<List<T>> LoadListAsync<T>(string streamName) where T : IAggregate, new()
    {
        // Implement this method according to your application's logic
        // Note: Getting all stream names might not be straightforward with gRPC
        throw new NotImplementedException();
    }

    public async Task DeleteStreamAsync(string streamName)
    {
        await _client.TombstoneAsync(streamName, StreamState.Any);
    }

    private bool IsRelevantEventForAggregate<T>(Type eventType)
    {
        if (AggregateEventMapping.TryGetValue(typeof(T), out var relevantEvents))
        {
            return relevantEvents.Contains(eventType);
        }
        return false;
    }

    private static readonly Dictionary<Type, List<Type>> AggregateEventMapping = new Dictionary<Type, List<Type>>
    {
        { typeof(Account), new List<Type> { typeof(AccountCreated), typeof(AccountDeleted), typeof(AccountUpdated) } },
        // Add other aggregates and their relevant event types
    };
}