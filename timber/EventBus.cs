/* EventBus.cs
 * 
 * This script implements an "Event Bus" -- a critical part of the Pub/Sub design pattern.
 * Developers should make heavy use of the Subscribe() and Publish() methods below to receive and send
 * instances of your own, custom "event" classes between systems. This "loosely couples" the systems, preventing spaghetti.
 * 
 * Please find an example usage of Publish() in ScorePointOnTouch.cs
 * Please find an example, custom Event class in ScorePointOnTouch.cs
 * Please find an example usage of Subscribe() in ScoreUI.cs
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class EventBus
{
    static Dictionary<Type, IList> _topics = new Dictionary<Type, IList>();

    public static void Publish<T>(T published_event)
    {
        /* Use type T to identify correct subscriber list (correct "topic") */
        Type t = typeof(T);

        if (_topics.ContainsKey(t))
        {
            IList subscriber_list = new List<Subscription<T>>(_topics[t].Cast<Subscription<T>>());

            /* iterate through the subscribers and pass along the event T */
            /* This is a collection of subscriptions that have lost their target object. */
            List<Subscription<T>> orphaned_subscriptions = new List<Subscription<T>>();
            
            foreach (Subscription<T> s in subscriber_list)
            {
                if(s.callback.Target == null || s.callback.Target.Equals(null))
                {
                    /* This callback is hanging, as its target object was destroyed */
                    /* Collect this callback and remove it later */
                    orphaned_subscriptions.Add(s);

                } else
                {
                    s.callback(published_event);
                }
            }

            /* Unsubcribe orphaned subs that have had their target objects destroyed */
            foreach(Subscription<T> orphan_subscription in orphaned_subscriptions) {
                EventBus.Unsubscribe<T>(orphan_subscription);
            }

        } else
        {

        }
    }

    public static Subscription<T> Subscribe<T>(Action<T> callback)
    {
        /* Determine event type so we can find the correct subscriber list */
        Type t = typeof(T);
        Subscription<T> new_subscription = new Subscription<T>(callback);

        /* If a subscriber list doesn't exist for this event type, create one */
        if (!_topics.ContainsKey(t))
            _topics[t] = new List<Subscription<T>>();

        _topics[t].Add(new_subscription);

        return new_subscription;
    }

    public static void Unsubscribe<T>(Subscription<T> subscription)
    {
        Type t = typeof(T);

        if (_topics.ContainsKey(t) && _topics[t].Count > 0)
        {
            _topics[t].Remove(subscription);

        } else
        {

        }
    }
}

/* A "handle" type that is returned when the EventBus.Subscribe() function is used.
 * Use this handle to unsubscribe if you wish via EventBus.Unsubscribe */
public class Subscription<T>
{
    public Action<T> callback { get; private set; }
    public Subscription(Action<T> _callback)
    {
        callback = _callback;
    }

    ~Subscription()
    {
        EventBus.Unsubscribe<T>(this);
    }
}
