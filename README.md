# EventManager

💌 An EventManager class that supports many events with generic payloads.

This is an extension of the Unity tutorial ["Creating a Simple Messaging System - April 2015 Live Training"](https://learn.unity.com/tutorial/create-a-simple-messaging-system-with-events#) where they make an Event Manager class.

A drawback of the Event Manager in that tutorial is that it doesn't pass any sort of payload along with the events. There are ways of using UnityEvents/delegates with an arbitrary amount of generic arguments, but the tutorial's implementation restricted the Event Manager to firing only a single kind of event.

**The Event Manager class in this repository is capable of firing an arbitrary amount of different event types that each have their own generic payloads.**

## Usage

Besides copying the included classes and pasting them in your own project, I'll explain the architecture of these classes.

- `class Program`: Just a class to demonstrate using the `EventManager`.
- `class EventManager`: The single source of truth for listeners added to events. All payloads are of `object` type.
- `class Event<Payload>`: Derive from this class with your own events. Automatically handles casting from `EventManager`'s `object` type to whichever specific payload type the derived class implements. Use `event.Add/RemoveListener` & `event.Trigger` to add, remove, and trigger events.
- `class ItemPickedUpEvent: Event<Item>`: An example derived class from `Event<Payload>`. The `Event<Payload>` base class will handle casting the `EventManager`'s `object` payloads to and from the derived classes specific payload type.
