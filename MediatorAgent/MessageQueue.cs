using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace MediatorAgent
{
    public class MessageQueue<T>
    {
        // queue stores the messages meant for a particular inboxid in memory
        // in FIFO order. Subscribers to a particular inbox can then consume
        // messages meant for it in the order they are received
        //private readonly ConcurrentDictionary<string, BlockingCollection<T>> queue;
        private readonly ConcurrentDictionary<string, Subject<T>> queue;

        public MessageQueue()
        {
            //queue = new ConcurrentDictionary<string, BlockingCollection<T>>();
            queue = new ConcurrentDictionary<string, Subject<T>>();
        }

        public IObservable<T> GetObservableForInbox(string inboxId)
        {
            var collection = queue.GetOrAdd(inboxId, new Subject<T>());

            // Cannot use collection.GetConsumingEnumerable() directly since
            // disposing a subscriber may occur when the Enumerable is blocked
            // at IEnumerable.MoveNext.
            // https://github.com/dotnet/reactive/issues/341
            /*
            return Observable.Defer(() =>
            {
                var cts = new CancellationTokenSource();
                return Observable.Using(
                    () => Disposable.Create(cts.Cancel),
                    _ => collection.GetConsumingEnumerable(cts.Token).ToObservable(TaskPoolScheduler.Default)
                );
            });
            */
            return collection;
        }

        public void enqueue(string inboxId, T message)
        {
	    Subject<T> collection;
	    foreach(var key in queue.Keys) {
                System.Diagnostics.Debug.WriteLine("Established connections with " + key);
            }
            System.Diagnostics.Debug.WriteLine("Trying to find " + inboxId);
            var ok = queue.TryGetValue(inboxId, out collection);
            if (!ok)
            {
                System.Diagnostics.Debug.Print("Subject not present!!");
		return;
            }
            collection.OnNext(message);
	    System.Diagnostics.Debug.WriteLine("Enqued message to the queue for inbox: " + inboxId);
        }
    }
}
