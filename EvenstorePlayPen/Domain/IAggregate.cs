namespace EvenstorePlayPen.Domain;

public interface IAggregate
{
    void ApplyEvent(object @event);
}