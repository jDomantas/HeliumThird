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
            [0] = typeof(ChatMessage),
            [1] = typeof(MapData),
            [2] = typeof(EntityUpdate),
            [3] = typeof(EntityRemoval),
            [4] = typeof(ChangeMap),
            [5] = typeof(PlayerInput),
            [6] = typeof(ControlledEntityChanged)
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
        /// Write events to network message
        /// </summary>
        /// <param name="msg">Network message</param>
        /// <param name="events">Collection of events to be written</param>
        public static void SerializeEvents(NetOutgoingMessage msg, IEnumerable<Event> events)
        {
            if (msg == null) throw new ArgumentNullException(nameof(msg));
            if (!events.Any()) throw new ArgumentException(nameof(events) + " must not be empty");
            if (events.Contains(null)) throw new ArgumentException(nameof(events) + " must not contain null");

            int eventCount = events.Count();
            
            if (eventCount > 255) throw new Exception("cannot serialize more than 255 events");

            msg.Write((byte)eventCount);
            
            foreach (var e in events)
            {
                long start = msg.Position;

                if (!EventIDs.ContainsKey(e.GetType()))
                    throw new Exception($"cannot serialize event {e.GetType()}");

                msg.Write(EventIDs[e.GetType()]);
                e.Serialize(msg);
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

            try
            {
                int eventCount = msg.ReadByte();
                Event[] result = new Event[eventCount];
                object[] constructorParams = new object[] { msg, sender };

                for (int i = 0; i < eventCount; i++)
                {
                    byte eventTypeID = msg.ReadByte();
                    if (!EventTypes.ContainsKey(eventTypeID))
                        throw new Exception($"unknown event type ID: {eventTypeID}");

                    var type = EventTypes[eventTypeID];
                    var constructor = type.GetConstructor(new Type[] { typeof(NetIncomingMessage), typeof(Player) });
                    if (constructor == null)
                        throw new Exception($"cannot deserialize event {type} as it does not have the necessary constructor");
                    
                    var deserializedEvent = (Event)constructor.Invoke(constructorParams);
                    
                    result[i] = deserializedEvent;
                }

                return result;
            }
            catch (Exception e)
            {
                // should only happen when event is deserialized using incorrect amount of data
                System.Diagnostics.Debug.WriteLine("Packet deserialization error, dropping messages");
                System.Diagnostics.Debug.WriteLine($"Error: {e.Message}");
                return new Event[0];
            }
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
