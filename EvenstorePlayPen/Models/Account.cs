using EvenstorePlayPen.Domain;

namespace EvenstorePlayPen.Models;

public class Account : IEvent, IAggregate
{
    public String Type { get; set; }

    public String AccountHolderName { get; set; }
    
    public AccountBalance AccountBalance { get; set; }
    
    public bool IsDeleted { get; set; }
    
    public void ApplyEvent(object @event)
    {
        switch (@event)
        {
            case AccountCreated createdEvent:
                Type = createdEvent.Account.Type;
                AccountBalance = createdEvent.Account.AccountBalance;
                AccountHolderName = createdEvent.Account.AccountHolderName;
                IsDeleted = createdEvent.Account.IsDeleted;
                Id = createdEvent.Account.Id;
                break;
            case AccountUpdated updatedEvent:
                Type = updatedEvent.Account.Type;
                AccountBalance = updatedEvent.Account.AccountBalance;
                AccountHolderName = updatedEvent.Account.AccountHolderName;
                IsDeleted = updatedEvent.Account.IsDeleted;
                Id = updatedEvent.Account.Id;
                break;
            case AccountDeleted deletedEvent:
                Id = deletedEvent.Id;
                IsDeleted = true;
                break;
            default:
                break;
        }
    }
}

public class AccountBalance
{
    public decimal Balance { get; set; }
}