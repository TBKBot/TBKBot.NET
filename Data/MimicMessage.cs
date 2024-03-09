using MongoDB.Bson.Serialization.Attributes;

namespace TBKBot.Data
{
    public class MimicMessage
    {
        [BsonId]
        public ulong Id { get; set; }
        public string Content { get; set; }
        public ulong ImpersonatorId { get; set; }
        public string ImpersonatorUsername { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
