using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;

namespace HeliumThird.Events
{
    public static class Serializer
    {
        private static Dictionary<byte, Type> EventTypes = new Dictionary<byte, Type>()
        {
            [0] = typeof(HeliumThird.Events.ChatMessage)
        };

        private static Dictionary<Type, byte> EventIDs;
        private static Type[] ConstructorParamTypes = new Type[] { typeof(NetIncomingMessage), typeof(Player) };

        static Serializer()
        {
            EventIDs = new Dictionary<Type, byte>();
            foreach (var entry in EventTypes)
            {
                if (EventIDs.ContainsKey(entry.Value))
                    throw new Exception($"event {entry.Value} has multiple ids");

                EventIDs.Add(entry.Value, entry.Key);
            }
        }

        /// <summary>
        /// Write events to empty network message
        /// </summary>
        /// <param name="msg">Network message</param>
        /// <param name="events">Collection of events to be written</param>
        public static void SerializeEvents(NetOutgoingMessage msg, IEnumerable<Event> events)
        {
            if (msg == null) throw new ArgumentNullException(nameof(msg));
            if (msg.Position != 0) throw new ArgumentException(nameof(msg) + " must be empty");
            if (!events.Any()) throw new ArgumentException(nameof(events) + " must not be empty");
            if (events.Contains(null)) throw new ArgumentException(nameof(events) + " must not contain null");

            int eventCount = events.Count();
            
            if (eventCount > 255) throw new Exception("cannot serialize more than 255 events");

            msg.Write((byte)eventCount);
            foreach (var e in events)
                msg.Write(0);

            int index = 0;
            foreach (var e in events)
            {
                long start = msg.Position;

                if (!EventIDs.ContainsKey(e.GetType()))
                    throw new Exception($"cannot serialize event {e.GetType()}");

                msg.Write(EventIDs[e.GetType()]);
                e.Serialize(msg);

                long end = msg.Position;
                long length = end - start;
                if (length > int.MaxValue)
                    throw new Exception($"event {e.GetType()} is too large, size: {length} bits");
                
                msg.Position = 8 + index * 32;
                msg.Write((int)length);
                msg.Position = end;
            }
        }

        /// <summary>
        /// Read events from network message and set their sender
        /// </summary>
        /// <param name="msg">Network message</param>
        /// <param name="sender">Sender</param>
        /// <returns>Collection of events</returns>
        internal static IEnumerable<Event> DeserializeEvents(NetIncomingMessage msg, Player sender)
        {
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            int eventCount = msg.ReadByte();
            Event[] result = new Event[eventCount];
            long currentEventStart = 8 + 32 * eventCount;
            object[] constructorParams = new object[] { msg, sender };

            for (int i = 0; i < eventCount; i++)
            {
                msg.Position = 8 + 32 * i;
                long length = msg.ReadInt32();
                msg.Position = currentEventStart;

                byte eventTypeID = msg.ReadByte();
                if (!EventTypes.ContainsKey(eventTypeID))
                    throw new Exception($"unknown event type ID: {eventTypeID}");

                var type = EventTypes[eventTypeID];
                var constructor = type.GetConstructor(new Type[] { typeof(NetIncomingMessage), typeof(Player) });
                if (constructor == null)
                    throw new Exception($"cannot deserialize event {type} as it does not have the necessary constructor");

#warning Should crash if deserializer reads past the end of the message
                var deserializedEvent = (Event)constructor.Invoke(constructorParams);

                //if (msg.Position != currentEventStart + length)
                //    throw new Exception($"event {type} used incorrect amount of data when deserializing " +
                //                        $"(expected: {length * 8} bits, used: {msg.Position - currentEventStart * 8}");

                result[i] = deserializedEvent;
                currentEventStart = msg.Position;
            }

            return result;
        }

        /// <summary>
        /// Read events from network message
        /// </summary>
        /// <param name="msg">Network message</param>
        /// <returns>Collection of events</returns>
        public static IEnumerable<Event> DeserializeEvents(NetIncomingMessage msg)
        {
            return DeserializeEvents(msg, null);
        }
    }
}
