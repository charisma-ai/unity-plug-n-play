namespace CharismaSDK.PlugNPlay
{
    internal class SetPlayerSpeakFunction : MetadataFunction
    {
        public override string MetadataId => "set-player-speak";

        public override void Execute(string metadataValue)
        {
            bool repliesEnabled = metadataValue == "true";

            var player = Dependencies.PlaythroughEntities.GetPlayerEntity();

            if (player != default)
            {
                player.SetPlayerInputFieldActive(repliesEnabled);
            }
        }
    }
}