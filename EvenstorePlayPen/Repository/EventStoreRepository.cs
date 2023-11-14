using System.Text;
using EvenstorePlayPen.Domain;
using EvenstorePlayPen.Models;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace EvenstorePlayPen.Repository;

public class EventStoreRepository
{
    private readonly IEventStoreConnection _connection;
    private readonly IConfiguration _configuration;

    public EventStoreRepository(IConfiguration configuration)
    {
        _configuration = configuration;
        _connection = EventStoreConnection.Create(configuration["eventStore:connectionString"],
            ConnectionSettings.Create().KeepReconnecting(), "accounts-manager");
        _connection.ConnectAsync();
    }
    public async Task AppendEvents(string streamName, object data)
    {
        var eventData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
        var eventPayload = new EventData(Guid.NewGuid(), data.GetType().AssemblyQualifiedName, true, eventData, null);
        await _connection.AppendToStreamAsync(streamName, events: eventPayload, expectedVersion: ExpectedVersion.Any);
    }
    
    public async Task<T> LoadAsync<T>(string streamName) where T : IAggregate, new()
    {
        var aggregate = new T();
        StreamEventsSlice currentSlice;
        long nextSliceStart = StreamPosition.Start;

        do
        {
            currentSlice = await _connection.ReadStreamEventsForwardAsync(streamName, nextSliceStart, 200, false);
            nextSliceStart = currentSlice.NextEventNumber;

            foreach (var resolvedEvent in currentSlice.Events)
            {
                // Deserialize the event data into the specific event type
                var eventType = Type.GetType(resolvedEvent.Event.EventType);
                var eventData = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Event.Data), eventType);

                // Apply the event to the aggregate
                aggregate.ApplyEvent(eventData);
                // Apply the event to the aggregate
            }
        } while (!currentSlice.IsEndOfStream);

        return aggregate;
    }
    
    public async Task<List<T>> LoadListAsync<T>(string streamName) where T : IAggregate, new()
    {
        var aggregates = new Dictionary<string, T>();

        AllEventsSlice allEvents = await _connection.ReadAllEventsForwardAsync(Position.Start, 4096, false);

        foreach (var resolvedEvent in allEvents.Events)
        {
            try
            {
                string streamId = resolvedEvent.Event.EventStreamId;
                var eventType = Type.GetType(resolvedEvent.Event.EventType);

                if (IsRelevantEventForAggregate<T>(eventType))
                {
                    // Ensure an aggregate exists for each stream ID
                    if (!aggregates.ContainsKey(streamId))
                    {
                        aggregates[streamId] = new T();
                    }
                
                    var eventData = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Event.Data), eventType);

                    // Apply the event to the appropriate aggregate
                    aggregates[streamId].ApplyEvent(eventData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
        
        return aggregates.Values.ToList();
    }
    
    public async Task DeleteStreamAsync(string streamName)
    {
        await _connection.DeleteStreamAsync(streamName, ExpectedVersion.Any);
    }
    
    private static readonly Dictionary<Type, List<Type>> AggregateEventMapping = new Dictionary<Type, List<Type>>
    {
        { typeof(Account), new List<Type> { typeof(AccountCreated), typeof(AccountDeleted), typeof(AccountUpdated) } },
        // Add other aggregates and their relevant event types
    };

    private bool IsRelevantEventForAggregate<T>(Type eventType)
    {
        if (AggregateEventMapping.TryGetValue(typeof(T), out var relevantEvents))
        {
            return relevantEvents.Contains(eventType);
        }
        return false;
    }
}