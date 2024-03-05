using System;
using System.Collections.Generic;
using System.IO;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using TBKBot.Models;

namespace TBKBot.Models
{
    public class GuildMember
    {
        [BsonId]
        public ulong Id { get; set; }
        public string Username { get; set; }
        public int Money { get; set; }
        public DateTime? Birthday { get; set; }
    }
}