
namespace CharismaSDK.PlugNPlay
{
    public class MetadataDependencies : IMetadataDependencies
    {
        public IPlaythroughEntities PlaythroughEntities => _playthroughEntities;

        private IPlaythroughEntities _playthroughEntities;

        public MetadataDependencies(IPlaythroughEntities entities)
        {
            _playthroughEntities = entities;
        }
    }
}
