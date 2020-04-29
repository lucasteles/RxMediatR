namespace MediatR.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract class NotificationHandlerWrapper
    {
        protected static Subject<INotification> notifier = new Subject<INotification>();
        internal static IObservable<INotification> Notifications => notifier;

        public abstract Task Handle(INotification notification, CancellationToken cancellationToken, ServiceFactory serviceFactory,
                                    Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish);
    }

    internal class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
        where TNotification : INotification
    {

        public override Task Handle(INotification notification, CancellationToken cancellationToken, ServiceFactory serviceFactory,
                                    Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish)
        {
            var handlers = serviceFactory
                .GetInstances<INotificationHandler<TNotification>>()
                .Select(x => new Func<INotification, CancellationToken, Task>((theNotification, theToken) => x.Handle((TNotification) theNotification, theToken)));

            notifier.OnNext(notification);
            var published = publish(handlers, notification, cancellationToken);

            return published;
        }
    }
}