using MySqlConnector;
using System.Data;
using System.Diagnostics;
using MauiApp2.Models;
using MauiApp2.ViewModels;
using MenuItem = MauiApp2.Models.MenuItem;

namespace MauiApp2.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<MySqlConnection> GetConnectionAsync()
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        // Méthode modifiée pour inclure la table Users
        public async Task InitializeDatabaseAsync()
{
    try
    {
        using var connection = await GetConnectionAsync();
        Debug.WriteLine("Connexion réussie: Base de données connectée!");
        using var command = connection.CreateCommand();
        
        // Vérifier si la table Orders existe déjà
        command.CommandText = "SHOW TABLES LIKE 'Orders'";
        var tableExists = await command.ExecuteScalarAsync();
        
        if (tableExists != null)
        {
            // Table existe, vérifions la structure
            command.CommandText = "DESCRIBE Orders";
            using var reader = await command.ExecuteReaderAsync();
            bool hasUserIdColumn = false;
            
            while (await reader.ReadAsync())
            {
                string columnName = reader.GetString(0);
                if (columnName.Equals("UserId", StringComparison.OrdinalIgnoreCase))
                {
                    hasUserIdColumn = true;
                    break;
                }
            }
            reader.Close();
            
            if (!hasUserIdColumn)
            {
                // Ajouter la colonne UserId si nécessaire
                Debug.WriteLine("Ajout de la colonne UserId à la table Orders");
                command.CommandText = "ALTER TABLE Orders ADD COLUMN UserId INT";
                await command.ExecuteNonQueryAsync();
            }
        }
        else
        {
            // Créer toutes les tables
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Categories (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(100) NOT NULL,
                Description TEXT
            );

            CREATE TABLE IF NOT EXISTS MenuItems (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(100) NOT NULL,
                Description TEXT,
                Price DECIMAL(10,2) NOT NULL,
                CategoryId INT NOT NULL,
                IsAvailable BOOLEAN DEFAULT TRUE,
                ImagePath VARCHAR(255),
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
            );

            CREATE TABLE IF NOT EXISTS Orders (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                OrderDate DATETIME NOT NULL,
                TableNumber INT NOT NULL,
                Status VARCHAR(50) NOT NULL,
                UserId INT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS OrderItems (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                OrderId INT NOT NULL,
                MenuItemId INT NOT NULL,
                ItemName VARCHAR(100) NOT NULL,
                Quantity INT NOT NULL,
                Price DECIMAL(10,2) NOT NULL,
                SpecialRequests TEXT,
                FOREIGN KEY (OrderId) REFERENCES Orders(Id),
                FOREIGN KEY (MenuItemId) REFERENCES MenuItems(Id)
            );

            CREATE TABLE IF NOT EXISTS Reservations (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                CustomerName VARCHAR(100) NOT NULL,
                PhoneNumber VARCHAR(20),
                NumberOfGuests INT NOT NULL,
                ReservationDate DATE NOT NULL,
                ReservationTime TIME NOT NULL,
                TableNumber INT NOT NULL,
                Status VARCHAR(50) NOT NULL,
                Notes TEXT
            );
            
            CREATE TABLE IF NOT EXISTS Users (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Username VARCHAR(50) NOT NULL UNIQUE,
                Password VARCHAR(255) NOT NULL,
                Email VARCHAR(100) NOT NULL UNIQUE,
                FullName VARCHAR(100),
                UserType ENUM('admin', 'client') NOT NULL,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            );";

            await command.ExecuteNonQueryAsync();
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Erreur de connexion: {ex.Message}");
        Debug.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = await GetConnectionAsync();
                return connection.State == System.Data.ConnectionState.Open;
            }
            catch (Exception)
            {
                return false;
            }
        }

// Ajouter cette méthode à votre classe DatabaseService
        public async Task<bool> RegisterUserAsync(User user)
        {
            try
            {
                // Vérification que les champs requis sont remplis
                if (string.IsNullOrEmpty(user.Username) ||
                    string.IsNullOrEmpty(user.Password) ||
                    string.IsNullOrEmpty(user.Email) ||
                    string.IsNullOrEmpty(user.UserType))
                {
                    return false;
                }

                // Vérification si l'utilisateur existe déjà
                if (await UserExistsAsync(user.Username, user.Email))
                {
                    return false;
                }

                using var connection = await GetConnectionAsync();
                using var command = connection.CreateCommand();

                // Nous devrions hacher le mot de passe avant de le stocker en base de données
                string hashedPassword = HashPassword(user.Password);

                command.CommandText = @"
            INSERT INTO Users (Username, Password, Email, FullName, UserType)
            VALUES (@Username, @Password, @Email, @FullName, @UserType)";

                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.AddWithValue("@Password", hashedPassword);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@FullName", user.FullName ?? "");
                command.Parameters.AddWithValue("@UserType", user.UserType.ToLower());

                int result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de l'inscription: {ex.Message}");
                return false;
            }
        }

// Vérifier si un utilisateur existe déjà
        private async Task<bool> UserExistsAsync(string username, string email)
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = connection.CreateCommand();

                command.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email";
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Email", email);

                object result = await command.ExecuteScalarAsync();
                int count = Convert.ToInt32(result);

                return count > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la vérification de l'utilisateur: {ex.Message}");
                return false;
            }
        }

// Une méthode simple pour hacher le mot de passe
// Remarque: Dans une application de production, utilisez un système de hachage plus robuste
        private string HashPassword(string password)
        {
            // Simple hachage avec SHA256 (pour démo seulement)
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

// Méthode pour la vérification des identifiants lors de la connexion
        public async Task<User> AuthenticateUserAsync(string usernameOrEmail, string password)
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = connection.CreateCommand();

                string hashedPassword = HashPassword(password);

                command.CommandText = @"
            SELECT Id, Username, Email, FullName, UserType 
            FROM Users 
            WHERE (Username = @Identifier OR Email = @Identifier) AND Password = @Password";

                command.Parameters.AddWithValue("@Identifier", usernameOrEmail);
                command.Parameters.AddWithValue("@Password", hashedPassword);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(2),
                        FullName = !reader.IsDBNull(3) ? reader.GetString(3) : string.Empty,
                        UserType = reader.GetString(4)
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de l'authentification: {ex.Message}");
                return null;
            }
        }
        
        public async Task<int> CreateOrderAsync(int tableNumber, int userId)
        {
            Debug.WriteLine("====== DÉBUT DE CRÉATION DE COMMANDE ======");
            Debug.WriteLine($"Connection string: {_connectionString}");
            try
            {
                Debug.WriteLine($"Tentative de création de commande pour: Table {tableNumber}, User {userId}");
        
                using var connection = await GetConnectionAsync();
                using var command = connection.CreateCommand();

                // Insérer directement la commande sans vérifications préalables
                command.CommandText = @"
            INSERT INTO Orders (OrderDate, TableNumber, Status, UserId)
            VALUES (NOW(), @TableNumber, 'en_attente', @UserId);
            SELECT LAST_INSERT_ID();";

                command.Parameters.AddWithValue("@TableNumber", tableNumber);
                command.Parameters.AddWithValue("@UserId", userId);

                Debug.WriteLine("Exécution de la requête d'insertion...");
                var result = await command.ExecuteScalarAsync();
        
                if (result != null)
                {
                    int orderId = Convert.ToInt32(result);
                    Debug.WriteLine($"Commande créée avec succès! ID: {orderId}");
                    return orderId;
                }
                else
                {
                    Debug.WriteLine("Aucun ID retourné après l'insertion");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la création de la commande: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return -1;
            }
        }

// Méthode pour ajouter des articles à une commande
        public async Task<bool> AddOrderItemsAsync(int orderId, List<CartItem> items)
{
    Debug.WriteLine($"Début AddOrderItemsAsync avec {items.Count} articles");
    
    try
    {
        using var connection = await GetConnectionAsync();
        Debug.WriteLine("Connexion établie pour AddOrderItemsAsync");
        
        using var transaction = await connection.BeginTransactionAsync();
        Debug.WriteLine("Transaction démarrée");

        try
        {
            foreach (var item in items)
            {
                Debug.WriteLine($"Traitement article: {item.ItemName}, Quantité: {item.Quantity}");
                
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = @"
                    INSERT INTO OrderItems (OrderId, MenuItemId, ItemName, Quantity, Price, SpecialRequests)
                    VALUES (@OrderId, @MenuItemId, @ItemName, @Quantity, @Price, @SpecialRequests)";

                command.Parameters.AddWithValue("@OrderId", orderId);
                command.Parameters.AddWithValue("@MenuItemId", item.MenuItemId);
                command.Parameters.AddWithValue("@ItemName", item.ItemName);
                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                command.Parameters.AddWithValue("@Price", item.Price);
                command.Parameters.AddWithValue("@SpecialRequests", item.SpecialRequests ?? "");

                int rowsAffected = await command.ExecuteNonQueryAsync();
                Debug.WriteLine($"Article ajouté: {rowsAffected} lignes affectées");
            }

            await transaction.CommitAsync();
            Debug.WriteLine("Transaction validée avec succès");
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Debug.WriteLine($"ERREUR SQL: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"ERREUR CONNEXION: {ex.Message}");
        return false;
    }
}

// Méthode pour récupérer les commandes d'un utilisateur
public async Task<List<Order>> GetUserOrdersAsync(int userId)
{
    var orders = new List<Order>();
    
    try
    {
        using var connection = await GetConnectionAsync();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT Id, OrderDate, TableNumber, Status 
            FROM Orders
            WHERE UserId = @UserId
            ORDER BY OrderDate DESC";

        command.Parameters.AddWithValue("@UserId", userId);

        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            orders.Add(new Order
            {
                Id = reader.GetInt32(0),
                OrderDate = reader.GetDateTime(1),
                TableNumber = reader.GetInt32(2),
                Status = reader.GetString(3)
            });
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Erreur lors de la récupération des commandes: {ex.Message}");
    }
    
    return orders;
}

// Méthode pour récupérer les articles d'une commande
public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId)
{
    var items = new List<OrderItem>();
    
    try
    {
        using var connection = await GetConnectionAsync();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT Id, MenuItemId, ItemName, Quantity, Price, SpecialRequests
            FROM OrderItems
            WHERE OrderId = @OrderId";

        command.Parameters.AddWithValue("@OrderId", orderId);

        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            items.Add(new OrderItem
            {
                Id = reader.GetInt32(0),
                MenuItemId = reader.GetInt32(1),
                ItemName = reader.GetString(2),
                Quantity = reader.GetInt32(3),
                Price = reader.GetDecimal(4),
                SpecialRequests = reader.IsDBNull(5) ? "" : reader.GetString(5)
            });
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Erreur lors de la récupération des articles de commande: {ex.Message}");
    }
    
    return items;
}


    }


}