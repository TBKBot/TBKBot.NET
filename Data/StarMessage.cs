using MongoDB.Bson.Serialization.Attributes;

namespace TBKBot.Models
{
    public class StarMessage
    {
        [BsonId]
        public ulong Id { get; set; }
        public int Stars { get; set; }
        public ulong? AuthorId { get; set; }
        public ulong ChannelId { get; set; }
        public string Content { get; set; }
        public ulong BoardMessageId { get; set; }
    }
}
