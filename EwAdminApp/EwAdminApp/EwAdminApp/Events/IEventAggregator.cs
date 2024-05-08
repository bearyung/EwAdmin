using System;

namespace EwAdminApp.Events;

public interface IEventAggregator
{
    public void Publish<T>(T eventToPublish);
    public IObservable<T> GetEvent<T>();
}