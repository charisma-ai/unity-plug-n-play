using CharismaSDK.Events;
using System;

namespace CharismaSDK.PlugNPlay
{
    public static class PlaythroughHelpers
    {
        public static bool DoesMessageBelongToCharacter(Message message, CharismaPlaythroughActor actor)
        {
            if (message == default)
            {
                return false;
            }

            if (message.character == default)
            {
                return false;
            }

            var name = message.character.name;

            // if the name from the message is empty, then probably don't need to send it to subscribers
            if (name == "")
            {
                return false;
            }

            // compare lower case version of the names as there can be discrepancy in typecase
            if (string.Equals(actor.CharacterId, name, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
