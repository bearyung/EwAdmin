using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace EwAdminApp.Events;

public class EventAggregator : IEventAggregator
{
    // Stores subjects for each event type
    private readonly ConcurrentDictionary<Type, object> _subjects = new();
    
    // Publishes an event
    public void Publish<T>(T eventToPublish)
    {
        if (_subjects.TryGetValue(typeof(T), out var subject))
        {
            ((ISubject<T>)subject).OnNext(eventToPublish);
        }
    }
    
    // Gets the observable that subscribers can subscribe to
    public IObservable<T> GetEvent<T>()
    {
        var subject = (ISubject<T>)_subjects.GetOrAdd(typeof(T), new Subject<T>());
        return subject.AsObservable();
    }

}