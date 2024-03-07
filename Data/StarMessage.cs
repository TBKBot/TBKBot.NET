using DSharpPlus.Entities;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TBKBot.Models
{
    public class StarMessage
    {
        [BsonId]
        public ulong Id { get; set; }
        public int Stars { get; set; }
        public ulong AuthorId { get; set; }
        public ulong ChannelId { get; set; }
        public string Content { get; set; }
        public ulong BoardMessageId { get; set; }
    }
}
