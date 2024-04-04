using MongoDB.Driver;
using TBKBot.Models;

namespace TBKBot.Data
{
    public class DBEngine
    {
        private readonly IMongoClient Client;
        private readonly IMongoDatabase Database;

        public DBEngine(string conn)
        {
            Client = new MongoClient(conn);
            Database = Client.GetDatabase("tbkbot");
        }

        //member data
        public async Task SaveMemberAsync(GuildMember member)
        {
            var col = Database.GetCollection<GuildMember>("members");

            // Create a filter to match the document by its _id field
            var filter = Builders<GuildMember>.Filter.Eq(x => x.Id, member.Id);

            // Replace the existing document with the new one, or insert it if not found
            await col.ReplaceOneAsync(filter, member, new ReplaceOptions { IsUpsert = true });
        }

        public async Task<GuildMember> LoadMemberAsync(ulong id)
        {
            var col = Database.GetCollection<GuildMember>("members");

            var member = await col.Find(x => x.Id == id).SingleOrDefaultAsync();

            member ??= new GuildMember()
            {
                Id = id,
                Username = "",
                Money = 0,
                Bank = 0,
                Birthday = null
            };

            return member;
        }

        public async Task<List<GuildMember>> GetAllMembersAsync()
        {
            var col = Database.GetCollection<GuildMember>("members");

            // Retrieve all documents from the collection
            var members = await col.Find(_ => true).ToListAsync();

            return members;
        }

        //mimic data
        public async Task SaveMimicMessageAsync(MimicMessage message)
        {
            var col = Database.GetCollection<MimicMessage>("mimics");

            var filter = Builders<MimicMessage>.Filter.Eq(x => x.Id, message.Id);

            await col.ReplaceOneAsync(filter, message, new ReplaceOptions { IsUpsert = true });
        }

        public async Task<MimicMessage> FindMimicMessageAsync(ulong id)
        {
            var col = Database.GetCollection<MimicMessage>("mimics");

            var message = await col.Find(x => x.Id == id).SingleOrDefaultAsync();

            return message;
        }

        //starboard data
        public async Task SaveStarMessageAsync(StarMessage message)
        {
            var col = Database.GetCollection<StarMessage>("starboard");

            var filter = Builders<StarMessage>.Filter.Eq(x => x.Id, message.Id);

            await col.ReplaceOneAsync(filter, message, new ReplaceOptions { IsUpsert = true });
        }

        public async Task<StarMessage> LoadStarMessageAsync(ulong id)
        {
            var col = Database.GetCollection<StarMessage>("starboard");

            var message = await col.Find(x => x.Id == id).SingleOrDefaultAsync();

            return message;
        }
    }
}
