using MauiApp2.Models;
using MySqlConnector;
using System.Collections.ObjectModel;
using MenuItem = MauiApp2.Models.MenuItem;

namespace MauiApp2.Services
{
    public class MenuService
    {
        private readonly DatabaseService _databaseService;

        public MenuService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<ObservableCollection<Category>> GetCategoriesAsync()
        {
            var categories = new ObservableCollection<Category>();
            
            using var connection = await _databaseService.GetConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Description FROM Categories ORDER BY Name";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                categories.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty
                });
            }

            return categories;
        }

        public async Task<ObservableCollection<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId)
        {
            var menuItems = new ObservableCollection<MenuItem>();
            
            using var connection = await _databaseService.GetConnectionAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Description, Price, CategoryId, IsAvailable, ImagePath FROM MenuItems WHERE CategoryId = @CategoryId ORDER BY Name";
            command.Parameters.AddWithValue("@CategoryId", categoryId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                menuItems.Add(new MenuItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty,
                    Price = reader.GetDecimal(3),
                    CategoryId = reader.GetInt32(4),
                    IsAvailable = reader.GetBoolean(5),
                    ImagePath = !reader.IsDBNull(6) ? reader.GetString(6) : string.Empty
                });
            }

            return menuItems;
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            using var connection = await _databaseService.GetConnectionAsync();
            using var command = connection.CreateCommand();
            
            command.CommandText = "INSERT INTO Categories (Name, Description) VALUES (@Name, @Description)";
            command.Parameters.AddWithValue("@Name", category.Name);
            command.Parameters.AddWithValue("@Description", category.Description ?? (object)DBNull.Value);

            int result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }
        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            using var connection = await _databaseService.GetConnectionAsync();
            using var command = connection.CreateCommand();
    
            command.CommandText = "UPDATE Categories SET Name = @Name, Description = @Description WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", category.Id);
            command.Parameters.AddWithValue("@Name", category.Name);
            command.Parameters.AddWithValue("@Description", category.Description ?? (object)DBNull.Value);

            int result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            using var connection = await _databaseService.GetConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Supprimer d'abord les plats associés à cette catégorie
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "DELETE FROM MenuItems WHERE CategoryId = @CategoryId";
                    command.Parameters.AddWithValue("@CategoryId", id);
                    await command.ExecuteNonQueryAsync();
                }

                // Ensuite supprimer la catégorie
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "DELETE FROM Categories WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);
                    int result = await command.ExecuteNonQueryAsync();

                    if (result > 0)
                    {
                        await transaction.CommitAsync();
                        return true;
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            using var connection = await _databaseService.GetConnectionAsync();
            using var command = connection.CreateCommand();
    
            command.CommandText = "DELETE FROM MenuItems WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            int result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> AddMenuItemAsync(MenuItem menuItem)
        {
            using var connection = await _databaseService.GetConnectionAsync();
            using var command = connection.CreateCommand();
            
            command.CommandText = "INSERT INTO MenuItems (Name, Description, Price, CategoryId, IsAvailable, ImagePath) VALUES (@Name, @Description, @Price, @CategoryId, @IsAvailable, @ImagePath)";
            command.Parameters.AddWithValue("@Name", menuItem.Name);
            command.Parameters.AddWithValue("@Description", menuItem.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Price", menuItem.Price);
            command.Parameters.AddWithValue("@CategoryId", menuItem.CategoryId);
            command.Parameters.AddWithValue("@IsAvailable", menuItem.IsAvailable);
            command.Parameters.AddWithValue("@ImagePath", menuItem.ImagePath ?? (object)DBNull.Value);

            int result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }

        // Méthodes d'édition et de suppression...
        public async Task<bool> UpdateMenuItemAsync(MenuItem menuItem)
        {
            using var connection = await _databaseService.GetConnectionAsync();
            using var command = connection.CreateCommand();
            
            command.CommandText = "UPDATE MenuItems SET Name = @Name, Description = @Description, Price = @Price, CategoryId = @CategoryId, IsAvailable = @IsAvailable, ImagePath = @ImagePath WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", menuItem.Id);
            command.Parameters.AddWithValue("@Name", menuItem.Name);
            command.Parameters.AddWithValue("@Description", menuItem.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Price", menuItem.Price);
            command.Parameters.AddWithValue("@CategoryId", menuItem.CategoryId);
            command.Parameters.AddWithValue("@IsAvailable", menuItem.IsAvailable);
            command.Parameters.AddWithValue("@ImagePath", menuItem.ImagePath ?? (object)DBNull.Value);

            int result = await command.ExecuteNonQueryAsync();
            return result > 0;
        }
    }
}