
namespace CharismaSDK.PlugNPlay
{
    internal class SetPlayerSpeakFunction : MetadataFunction
    {
        public override string MetadataId => "set-player-speak";

        public override void Execute(string metadataValue)
        {
            if (metadataValue == "true" || metadataValue == "")
            {
                var player = Dependencies.PlaythroughEntities.GetPlayerEntity();

                if (player != default)
                {
                    player.SetReadyToReply();
                }
            }
        }
    }
}
