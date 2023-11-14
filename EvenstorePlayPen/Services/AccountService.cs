using EvenstorePlayPen.Models;
using EvenstorePlayPen.Repository;

namespace EvenstorePlayPen.Services;

public class AccountService
{
    private readonly EventStoreRepository _eventStoreRepository;

    public AccountService(EventStoreRepository eventStoreRepository)
    {
        _eventStoreRepository = eventStoreRepository;
    }

    public async Task AddAccount(Account account)
    {
        var accountCreated = new AccountCreated()
        {
            Account = account
        };
        await _eventStoreRepository.AppendEvents(GetStreamName(account.Id), accountCreated);
    }
    
    public async Task UpdateAccount(Account account)
    {
        var accountUpdated = new AccountUpdated()
        {
            Account = account
        };
        await _eventStoreRepository.AppendEvents(GetStreamName(account.Id), accountUpdated);
    }
    
    public async Task<List<Account>> GetAccounts()
    {
        return await _eventStoreRepository.LoadListAsync<Account>(GetStreamName());
    }
    
    public async Task DeleteAccount(Guid id)
    {
        var accountDeleted = new AccountDeleted()
        {
            Id= id
        };
        await _eventStoreRepository.AppendEvents(GetStreamName(id), accountDeleted);
    }
    
    public async Task<Account?> GetAccount(Guid id)
    {
        return await _eventStoreRepository.LoadAsync<Account>(GetStreamName(id));

    }
    public async Task DeleteStream(string streamName)
    {
        await _eventStoreRepository.DeleteStreamAsync(streamName);
    }
    
    private string GetStreamName(Guid? id = null)
    {
        var aggregateName = nameof(Account).ToLowerInvariant();

        if (id is not null)
        {
            return $"{aggregateName}_{id}";
        }
        else
        {
            return aggregateName;
        }
    }

}