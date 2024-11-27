namespace CharismaSDK.PlugNPlay
{
    internal class RequestInterruptionTextFunction : MetadataFunction
    {
        public override string MetadataId => "request-interruption-text";

        public override void Execute(string metadataValue)
        {
            if (metadataValue == "true" || metadataValue == "")
            {
                var player = Dependencies.PlaythroughEntities.GetPlayerEntity();

                if (player != default)
                {
                    player.ForceSendLastInterruption();
                }
            }
        }
    }
}