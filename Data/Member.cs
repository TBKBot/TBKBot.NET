﻿using MongoDB.Bson.Serialization.Attributes;

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