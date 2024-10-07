using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api.Services
{
    public class UserRepository
    {
        private readonly IMongoCollection<ApplicationUser> _users;

        public UserRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _users = database.GetCollection<ApplicationUser>("Users");
        }

        public async Task CreateAsync(ApplicationUser user) => await _users.InsertOneAsync(user);

        //Get user by mail
        public async Task<ApplicationUser?> GetByEmailAsync(string email) =>
            await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

        public async Task<List<ApplicationUser>> GetPendingApprovalUsersAsync() =>
            await _users.Find(u => !u.IsApproved).ToListAsync();


        // Method to approve customer
        public async Task<bool> ApproveCustomerAsync(string userId)
        {
            var filter = Builders<ApplicationUser>.Filter.Eq(u => u.UserId, userId);
            var update = Builders<ApplicationUser>.Update.Set(u => u.IsApproved, true);

            var result = await _users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;  // Returns true if the customer's status was updated
        }

        public async Task NotifyCSR() =>
            await _users.UpdateManyAsync(u => !u.IsApproved && !u.IsApproved, Builders<ApplicationUser>.Update.Set(u => u.IsApproved, true));

        //Get existing user IDs
        public async Task<bool> getExistingUserIds(String generatedId)
        {
            return await _users.Find(u => u.UserId == generatedId).AnyAsync();
        }

        //Get user by ID
        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _users.Find(u => u.UserId == userId).FirstOrDefaultAsync();
        }

        //Get all customers
        public async Task<List<ApplicationUser>> GetAllCustomersAsync()
        {
            return await _users.Find(u => u.Role == "Customer").ToListAsync();
        }

        //Update user
        public async Task UpdateAsync(ApplicationUser user)
        {
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

    }

}