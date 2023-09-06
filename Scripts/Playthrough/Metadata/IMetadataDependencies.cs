namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Interface for metadata dependencies
    /// NOTE: Interface is a bit redundant? not sure if we need it
    /// </summary>
    public interface IMetadataDependencies
    {
        // main dependency currently are the entities within the playthrough as metadata tends to interact with these directly
        // This can be expanded to include subsystems or other progress tracking for example
        public IPlaythroughEntities PlaythroughEntities { get; }
    }
}
