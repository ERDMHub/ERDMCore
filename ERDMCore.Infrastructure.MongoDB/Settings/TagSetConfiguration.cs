using MongoDB.Driver;

namespace ERDMCore.Infrastructure.MongoDB.Settings
{
    public class TagSetConfiguration
    {
        public List<Tag> Tags { get; set; } = new List<Tag>();

        public TagSet ToTagSet()
        {
            return new TagSet(Tags);
        }
    }
}
