using System;
using Xamarin.Forms;

namespace IUBH.TOR.Utilities.Messaging
{
    /// <summary>
    /// Provides a nicer interface around the Xamarin.Forms Messenger.
    /// See https://thomasbandt.com/a-nicer-messaging-interface-for-xamarinforms
    /// for more information and samples on how to use it.
    /// </summary>
    internal class FormsMessenger : IMessenger
    {
        public void Send<TMessage>(TMessage message, object sender = null) where TMessage : IMessage
        {
            if (sender == null)
            {
                sender = new object();
            }

            MessagingCenter.Send(sender, typeof(TMessage).FullName, message);
        }

        public void Subscribe<TMessage>(object subscriber, Action<object, TMessage> callback)
            where TMessage : IMessage
        {
            MessagingCenter.Subscribe(subscriber, typeof(TMessage).FullName, callback);
        }

        public void Unsubscribe<TMessage>(object subscriber) where TMessage : IMessage
        {
            MessagingCenter.Unsubscribe<object, TMessage>(subscriber, typeof(TMessage).FullName);
        }
    }
}
