// Code taken from Caliburn.micro
namespace XmlEditor.Applications.Helpers
{
    using System;
    using System.Windows.Threading;
    using System.Collections.Generic;
    using System.Linq;

    public class EventAggregationProvider
    {
        static EventAggregator instance;
        public static EventAggregator Instance
        {
            get { return instance ?? (instance = new EventAggregator()); }
        }
    }

    /// <summary>
    /// Enables easy marshalling of code to the UI thread.
    /// </summary>
    public static class Execute
    {
        private static Action<Action> executor = action => action();

        /// <summary>
        /// Initializes the framework using the current dispatcher.
        /// </summary>
        public static void InitializeWithDispatcher()
        {
            var dispatcher = Dispatcher.CurrentDispatcher;

            executor = action =>
            {
                if (dispatcher.CheckAccess())
                    action();
                else dispatcher.Invoke(action);
            };
        }

        /// <summary>
        /// Executes the action on the UI thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static void OnUIThread(this Action action)
        {
            executor(action);
        }
    }


    /// <summary>
    /// A marker interface for classes that subscribe to messages.
    /// </summary>
    public interface IHandle { }

    /// <summary>
    /// Denotes a class which can handle a particular type of message.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to handle.</typeparam>
    public interface IHandle<in TMessage> : IHandle
    {
        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Handle(TMessage message);
    }

    /// <summary>
    /// Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public interface IEventAggregator
    {
        /// <summary>
        /// Subscribes an instance to all events declared through implementations of <see cref="IHandle{T}"/>
        /// </summary>
        /// <param name="instance">The instance to subscribe for event publication.</param>
        void Subscribe(object instance);

        /// <summary>
        /// Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="instance">The instance to unsubscribe.</param>
        void Unsubscribe(object instance);

        /// <summary>
        /// Publishes a message.
        /// </summary>
        /// <typeparam name="T">The type of message being published.</typeparam>
        /// <param name="message">The message instance.</param>
        void Publish<T>(T message);
    }

    /// <summary>
    /// Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public class EventAggregator : IEventAggregator
    {
        readonly List<WeakReference> subscribers = new List<WeakReference>();

        /// <summary>
        /// Subscribes an instance to all events declared through implementations of <see cref="IHandle{T}"/>
        /// </summary>
        /// <param name="instance">The instance to subscribe for event publication.</param>
        public void Subscribe(object instance)
        {
            lock (subscribers)
            {
                if (subscribers.Any(reference => reference.Target == instance))
                    return;

                subscribers.Add(new WeakReference(instance));
            }
        }

        /// <summary>
        /// Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="instance">The instance to unsubscribe.</param>
        public void Unsubscribe(object instance)
        {
            lock (subscribers)
            {
                var found = subscribers
                    .FirstOrDefault(reference => reference.Target == instance);

                if (found != null)
                    subscribers.Remove(found);
            }
        }

        /// <summary>
        /// Publishes a message.
        /// </summary>
        /// <typeparam name="TMessage">The type of message being published.</typeparam>
        /// <param name="message">The message instance.</param>
        public void Publish<TMessage>(TMessage message)
        {
            WeakReference[] toNotify;
            lock (subscribers)
                toNotify = subscribers.ToArray();

            Execute.OnUIThread(() =>
            {
                var dead = new List<WeakReference>();

                foreach (var reference in toNotify)
                {
                    var target = reference.Target as IHandle<TMessage>;

                    if (target != null)
                        target.Handle(message);
                    else if (!reference.IsAlive)
                        dead.Add(reference);
                }
                if (dead.Count <= 0) return;
                lock (subscribers)
                    dead.Apply(x => subscribers.Remove(x));
            });
        }
    }
}