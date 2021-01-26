using System;
using System.Collections.Generic;

namespace EventManager
{
    class Program
    {
        static void Main(string[] args)
        {
			EventManager eventManager = new EventManager();
			
			// Create event and add some listeners.
			IntEvent intEvent = new IntEvent();
			intEvent.AddListener(eventManager, Program.Listener);
			intEvent.AddListener(eventManager, Program.Listener2);

			// Trigger the event.
			intEvent.Trigger(eventManager, 123);

            // Remember to remove the listeners.
            intEvent.RemoveListener(eventManager, Program.Listener);
            intEvent.RemoveListener(eventManager, Program.Listener2);

            // Pause the terminal so we can see the output.
            Console.ReadKey();
		}
		static void Listener(int payload)
		{
			Console.WriteLine("Program.Listener called with arg {0} of type {1}", payload, payload.GetType());
		}

		static void Listener2(int payload)
		{
			Console.WriteLine("Program.Listener2 called with arg {0} of type {1}", payload, payload.GetType());
		}
	}

	public class EventManager
	{

		// This is our dictionary of types and their registered listeners.
		// Listener payloads are just "objects". The specific events handle casting that to the right payload type.
		Dictionary<Type, Action<object>> eventListeners;

		public EventManager()
		{
			// Initialize the dictionary.
			this.eventListeners = new Dictionary<Type, Action<object>>();
		}

		public void AddListener(Type eventType, Action<object> listener)
		{
			Action<object> existingListener;
			this.eventListeners.TryGetValue(eventType, out existingListener);
			// If we already have a listener for this event type, then add our listener to it so
			// they are called along with the rest of the listeners when the event fires.
			if (existingListener != null)
			{
				existingListener += listener;
				// `existingDelegate += newDelegate` actually returns a reference to a new delegate, changing
				// what value existingListener references, so we must reassign the delegate in
				// this.eventListeners to this new delegate.
				this.eventListeners[eventType] = existingListener;
				Console.WriteLine("Added listener of type {0}, count: {1}", eventType, existingListener.GetInvocationList().Length);
			}
			else
			{
				// If we don't have an existing listener, we add a new one.	
				this.eventListeners.Add(eventType, listener);
				Console.WriteLine("Added listener of type {0}, count: 1", eventType);
			}
		}

		public void RemoveListener(Type eventType, Action<object> listener)
		{
			Action<object> existingListener;
			this.eventListeners.TryGetValue(eventType, out existingListener);
			if (existingListener != null)
			{
				existingListener -= listener;
				// If there's no more listeners, remove it from our dictionary.
				if (existingListener == null)
                {
					this.eventListeners.Remove(eventType);
					Console.WriteLine("Removed listener of type {0}, remaining count: 0", eventType);
				} else
                {
					// If there are some listeners left, then reassign the dictionary's delegate reference to this new delegate reference.
					this.eventListeners[eventType] = existingListener;
					Console.WriteLine("Removed listener of type {0}, remaining count: {1}", eventType, existingListener.GetInvocationList().Length);
				}
			}
		}

		public void TriggerEvent(Type eventType, object payload)
		{
			Action<object> listener;
			this.eventListeners.TryGetValue(eventType, out listener);
			// If there aren't any listeners to fire, just return early.
			if (listener == null)
			{
				return;
			}
			Console.WriteLine("Event Manager invoking {0} listeners of type {1} ", listener.GetInvocationList().Length, eventType);
			// We just pass the listeners the payload as an object. The listener will know what specific type to cast it to.
			listener(payload);
		}

	}

	// The job of this parent class is to handle all the casting from the Event Manager's "object"
	// event payloads & listeners to the derived class's specific payload type.
	public class Event<Payload>
	{

		// When event listeners for specific event types are added/removed via this class, this
		// dictionary lets us know which listeners in the EventManager's dictionary of
		// listeners should we remove.
		Dictionary<Action<Payload>, Action<object>> listenerWrappers;

		public Event()
		{
			// Init dictionary.
			this.listenerWrappers = new Dictionary<Action<Payload>, Action<object>>();
		}

		public void AddListener(EventManager eventManager, Action<Payload> listener)
		{
			// Return early if listener is already added.
			if (this.listenerWrappers.ContainsKey(listener))
			{
				return;
			}
			// This is the listener we give the Event Manager. It just calls our listener with the payload casted to our specific type.
			Action<object> listenerWrapper = (object payload) => { listener((Payload)payload); };
			// We add the listener wrapper to our dictionary so that we know which listener wrapper to remove
			// from the event manager when one of our own listeners requests to remove a listener.
			this.listenerWrappers.Add(listener, listenerWrapper);
			eventManager.AddListener(this.GetType(), listenerWrapper);
		}

		public void Trigger(EventManager eventManager, Payload payload)
		{
			Console.WriteLine("Triggering event of type {0} with payload {1} of type {2}", this.GetType(), payload, payload.GetType());
			// When we trigger the event in the event manager, we cast the payload to the compatible "object" type. This
			// is what the listener wrapper we made in AddListener will receive and cast back to our
			// appropriate payload type before sending it to our own listeners.
			eventManager.TriggerEvent(this.GetType(), (object)payload);
		}

		public void RemoveListener(EventManager eventManager, Action<Payload> listener)
		{
			Action<object> listenerWrapper;
			this.listenerWrappers.TryGetValue(listener, out listenerWrapper);
			// Return early if there's no listener to remove.
			if (listenerWrapper == null)
			{
				return;
			}
			this.listenerWrappers.Remove(listener);
			eventManager.RemoveListener(this.GetType(), listenerWrapper);
		}
	}

	// This is how you would create new events with varying event types. This is all that needs to be done.
	public class IntEvent : Event<int> { }

	// public class PlayerDiedEvent : Event<Player> { }
	// public class ItemPickedUpEvent : Event<Item> { }
	// public class ItemEquippedEvent : Event<EquipmentItem> { }
}
