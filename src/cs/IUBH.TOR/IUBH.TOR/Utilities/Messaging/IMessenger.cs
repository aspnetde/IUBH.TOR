using System;

namespace IUBH.TOR.Utilities.Messaging
{
    /// <summary>
    /// Provides a nicer interface around the Xamarin.Forms Messenger.
    /// See https://thomasbandt.com/a-nicer-messaging-interface-for-xamarinforms
    /// for more information and samples on how to use it.
    /// </summary>
    public interface IMessenger
    {
        void Send<TMessage>(TMessage message, object sender = null) where TMessage : IMessage;

        void Subscribe<TMessage>(object subscriber, Action<object, TMessage> callback)
            where TMessage : IMessage;

        void Unsubscribe<TMessage>(object subscriber) where TMessage : IMessage;
    }
}
