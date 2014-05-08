﻿using System;
using System.Collections.Generic;
using System.Messaging;
using MassTransit;
using MassTransit.Advanced;
using MassTransit.SubscriptionConfigurators;

namespace MassBussTesst
{
    class Szyna
    {
        private readonly List<Action<SubscriptionBusServiceConfigurator>> subscribtions
            = new List<Action<SubscriptionBusServiceConfigurator>>();

        public void Publish<T>(T message) where T : class
        {
            System.Diagnostics.Debug.WriteLine("Publish: " + message);
            Bus.Instance.Publish(message);
        }

        public void Subscribe<T>(IMessageSubscriber<T> subscriber) where T : class
        {
            subscribtions.Add(subs => subs.Handler<T>(subscriber.Handle).Permanent());
        }

        public void Initialize()
        {
            var address = new Uri("msmq://localhost/created_transactional");
            const string localName = @".\private$\created_transactional";

            if (MessageQueue.Exists(localName))
                MessageQueue.Delete(localName);

            Bus.Initialize(
                sbc =>
                {
                    sbc.UseMsmq(o => o.UseMulticastSubscriptionClient());
                    sbc.SetConcurrentReceiverLimit(1);
                    sbc.ReceiveFrom(address);
                    sbc.SetCreateMissingQueues(true);
                    sbc.SetCreateTransactionalQueues(true);
                    sbc.Subscribe(subs => subscribtions.ForEach(action => action(subs)));
                });
        }

        public static void Shutdown()
        {
            Bus.Shutdown();
        }
    }
}