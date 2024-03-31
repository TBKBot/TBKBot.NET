using MongoDB.Bson.Serialization.Attributes;

namespace TBKBot.Models
{
    public class GuildMember
    {
        [BsonId]
        public ulong Id { get; set; }
        public string Username { get; set; }
        public int Money { get; set; }
        public int Bank { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime? RespawnTime { get; set; }
        public DateTime? LastStealTime { get; set; }


        public bool AddMoney(int amount)
        {
            if (amount + Money > int.MaxValue)
            {
                return false;
            }

            Money += amount;

            return true;
        }

        public bool RemoveMoney(int amount)
        {
            if (amount > Money)
            {
                return false;
            }

            Money -= amount;

            return true;
        }

        public bool Deposit(int amount)
        {
            if (amount > Money || amount + Bank > int.MaxValue)
            {
                return false;
            }

            Money -= amount;
            Bank += amount;

            return true;
        }

        public bool Withdraw(int amount)
        {
            if (amount > Bank || amount + Money > int.MaxValue)
            {
                return false;
            }

            Money += amount;
            Bank -= amount;

            return true;
        }
    }
}