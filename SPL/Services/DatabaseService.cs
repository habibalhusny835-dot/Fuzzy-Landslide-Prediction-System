using SQLite;
using SPL.Model;

namespace SPL.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            string folderPath = @"D:\SPL_Database";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string dbPath = Path.Combine(folderPath, "spl_database.db3");

            _database = new SQLiteAsyncConnection(dbPath);

            _database.CreateTableAsync<UserAccount>().Wait();
            _database.CreateTableAsync<PredictionHistory>().Wait();
        }

        public string GetDatabasePath()
        {
            return _database.DatabasePath;
        }

        public async Task<int> RegisterUserAsync(UserAccount user)
        {
            user.CreatedAt = DateTime.Now;
            return await _database.InsertAsync(user);
        }

        public async Task<UserAccount?> GetUserByUsernameAsync(string username)
        {
            return await _database.Table<UserAccount>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();
        }

        public async Task<UserAccount?> LoginUserAsync(string username, string password, string role)
        {
            return await _database.Table<UserAccount>()
                .Where(u =>
                    u.Username == username &&
                    u.Password == password &&
                    u.Role == role)
                .FirstOrDefaultAsync();
        }

        public async Task<UserAccount?> GetUserForFaceAsync(string username, string role)
        {
            return await _database.Table<UserAccount>()
                .Where(u =>
                    u.Username == username &&
                    u.Role == role)
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserAccount>> GetAllUsersAsync()
        {
            return await _database.Table<UserAccount>()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> SavePredictionAsync(PredictionHistory history)
        {
            history.CreatedAt = DateTime.Now;
            return await _database.InsertAsync(history);
        }

        public async Task<List<PredictionHistory>> GetPredictionHistoriesAsync()
        {
            return await _database.Table<PredictionHistory>()
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> DeletePredictionAsync(PredictionHistory history)
        {
            return await _database.DeleteAsync(history);
        }
    }
}