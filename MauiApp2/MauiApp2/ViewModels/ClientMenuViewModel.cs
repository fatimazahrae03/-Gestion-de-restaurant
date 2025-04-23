using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using MauiApp2.Models;
using MauiApp2.Services;
using MauiApp2.Views;
using System.Linq;
using MenuItem = MauiApp2.Models.MenuItem;

namespace MauiApp2.ViewModels
{
    public class ClientMenuViewModel : BaseViewModel
    {
        private readonly MenuService _menuService;

        private Category _selectedCategory;
        private string _searchQuery;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<MenuItemWithQuantity> _menuItems;
        private ObservableCollection<MenuItemWithQuantity> _filteredMenuItems;
        private ObservableCollection<CartItem> _cartItems;
        private int _tableNumber;

        public ClientMenuViewModel(MenuService menuService, int tableNumber = 1)
        {
            _menuService = menuService;
            _tableNumber = tableNumber;

            // Initialiser les collections pour éviter les erreurs NullReferenceException
            _categories = new ObservableCollection<Category>();
            _menuItems = new ObservableCollection<MenuItemWithQuantity>();
            _filteredMenuItems = new ObservableCollection<MenuItemWithQuantity>();
            _cartItems = new ObservableCollection<CartItem>();

            LoadCategoriesCommand = new Command(async () => await LoadCategoriesAsync());
            LoadMenuItemsCommand = new Command(async () => await LoadMenuItemsAsync());
            SearchCommand = new Command(FilterMenuItems);
            IncreaseQuantityCommand = new Command<MenuItemWithQuantity>(IncreaseQuantity);
            DecreaseQuantityCommand = new Command<MenuItemWithQuantity>(DecreaseQuantity);
            AddToCartCommand = new Command<MenuItemWithQuantity>(AddToCart);
            ViewCartCommand = new Command(async () => await ViewCartAsync());

            // Utiliser MainThread pour les opérations UI
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await LoadCategoriesAsync();
                await LoadMenuItemsAsync();
            });
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<MenuItemWithQuantity> MenuItems
        {
            get => _menuItems;
            set => SetProperty(ref _menuItems, value);
        }

        public ObservableCollection<MenuItemWithQuantity> FilteredMenuItems
        {
            get => _filteredMenuItems;
            set => SetProperty(ref _filteredMenuItems, value);
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    // Force le rechargement des plats
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await LoadMenuItemsAsync();
                        FilterMenuItems();
                    });
                }
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    FilterMenuItems();
                }
            }
        }

        public ICommand LoadCategoriesCommand { get; }
        public ICommand LoadMenuItemsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand ViewCartCommand { get; }

        private async Task LoadCategoriesAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                var categories = await _menuService.GetCategoriesAsync();

                // Vérifier si nous avons reçu des catégories
                if (categories != null && categories.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Catégories chargées: {categories.Count}");
                    Categories = new ObservableCollection<Category>(categories);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Aucune catégorie n'a été chargée");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
                await Shell.Current.DisplayAlert("Erreur", "Impossible de charger les catégories.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadMenuItemsAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                var items = new ObservableCollection<MenuItemWithQuantity>();

                // Charger tous les éléments du menu, indépendamment de la catégorie
                var allCategories = await _menuService.GetCategoriesAsync();
                System.Diagnostics.Debug.WriteLine($"Chargement des plats pour {allCategories.Count} catégories");

                foreach (var category in allCategories)
                {
                    var menuItems = await _menuService.GetMenuItemsByCategoryAsync(category.Id);
                    System.Diagnostics.Debug.WriteLine(
                        $"Catégorie {category.Name} (ID: {category.Id}): {menuItems.Count} plats trouvés");

                    foreach (var item in menuItems)
                    {
                        // S'assurer que CategoryId est correctement défini
                        System.Diagnostics.Debug.WriteLine($"Plat: {item.Name}, CategoryId: {item.CategoryId}");

                        // Si CategoryId n'est pas défini dans l'item, utiliser celui de la catégorie en cours
                        int categoryId = item.CategoryId > 0 ? item.CategoryId : category.Id;

                        items.Add(new MenuItemWithQuantity
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Description = item.Description,
                            Price = item.Price,
                            CategoryId = categoryId, // Utiliser la valeur corrigée
                            IsAvailable = item.IsAvailable,
                            ImagePath = item.ImagePath,
                            Quantity = 1
                        });
                    }
                }

                MenuItems = new ObservableCollection<MenuItemWithQuantity>(items);
                System.Diagnostics.Debug.WriteLine($"Total des plats chargés: {MenuItems.Count}");

                // Appliquer le filtrage initial
                FilterMenuItems();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading menu items: {ex.Message}");
                await Shell.Current.DisplayAlert("Erreur", "Impossible de charger les plats.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FilterMenuItems()
        {
            if (MenuItems == null)
            {
                System.Diagnostics.Debug.WriteLine("MenuItems est null lors du filtrage");
                FilteredMenuItems = new ObservableCollection<MenuItemWithQuantity>();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Filtrage des plats. Nombre total: {MenuItems.Count}");
            IEnumerable<MenuItemWithQuantity>
                filtered = MenuItems.ToList(); // Créer une copie pour éviter les problèmes de référence

            // Filtrer par catégorie si une catégorie est sélectionnée
            if (SelectedCategory != null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Filtrage par catégorie: {SelectedCategory.Name} (ID: {SelectedCategory.Id})");
                filtered = filtered.Where(item => item.CategoryId == SelectedCategory.Id);
                System.Diagnostics.Debug.WriteLine($"Après filtrage par catégorie: {filtered.Count()} plats");
            }

            // Filtrer par termes de recherche si une recherche est effectuée
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string searchLower = SearchQuery.ToLowerInvariant();
                System.Diagnostics.Debug.WriteLine($"Filtrage par recherche: '{searchLower}'");
                filtered = filtered.Where(item =>
                    item.Name.ToLowerInvariant().Contains(searchLower) ||
                    (item.Description != null && item.Description.ToLowerInvariant().Contains(searchLower)));
                System.Diagnostics.Debug.WriteLine($"Après filtrage par recherche: {filtered.Count()} plats");
            }

            // Ne montrer que les plats disponibles
            filtered = filtered.Where(item => item.IsAvailable);
            System.Diagnostics.Debug.WriteLine(
                $"Plats disponibles: {filtered.Count(item => item.IsAvailable)} sur {filtered.Count()}");

            // Mettre à jour la liste filtrée et notifier les changements
            FilteredMenuItems = new ObservableCollection<MenuItemWithQuantity>(filtered);

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.DisplayAlert("Diagnostic",
                    $"Catégorie: {SelectedCategory?.Name ?? "Aucune"}\n" +
                    $"Nombre total de plats: {MenuItems.Count}\n" +
                    $"Nombre de plats filtrés: {filtered.Count()}",
                    "OK");
            });

            // Notifier explicitement le changement
            OnPropertyChanged(nameof(FilteredMenuItems));
        }

        private void IncreaseQuantity(MenuItemWithQuantity item)
        {
            if (item != null)
            {
                item.Quantity++;
                // Notifier que la propriété a changé
                OnPropertyChanged(nameof(FilteredMenuItems));
            }
        }

        private void DecreaseQuantity(MenuItemWithQuantity item)
        {
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
                // Notifier que la propriété a changé
                OnPropertyChanged(nameof(FilteredMenuItems));
            }
        }

        private void AddToCart(MenuItemWithQuantity item)
        {
            if (item != null && item.Quantity > 0)
            {
                // Vérifier si l'article est déjà dans le panier
                var existingItem = _cartItems.FirstOrDefault(i => i.MenuItemId == item.Id);

                if (existingItem != null)
                {
                    // Mettre à jour la quantité si l'article existe déjà
                    existingItem.Quantity += item.Quantity;
                }
                else
                {
                    // Ajouter un nouvel article au panier
                    _cartItems.Add(new CartItem
                    {
                        MenuItemId = item.Id,
                        ItemName = item.Name,
                        Price = item.Price,
                        Quantity = item.Quantity
                    });
                }

                // Réinitialiser la quantité dans l'affichage après l'ajout au panier
                item.Quantity = 1;

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.DisplayAlert("Succès", $"{item.Name} ajouté au panier", "OK");
                });
            }
        }

        // Dans ClientMenuViewModel.cs
        private async Task ViewCartAsync()
        {
            if (_cartItems.Count == 0)
            {
                await Shell.Current.DisplayAlert("Panier vide",
                    "Ajoutez des articles à votre panier avant de continuer.", "OK");
                return;
            }

            // Ajoute des logs pour déboguer
            Debug.WriteLine($"Navigation vers CartPage avec {_cartItems.Count} articles");
            foreach (var item in _cartItems)
            {
                Debug.WriteLine($"- {item.ItemName}: {item.Quantity} x {item.Price}€");
            }

            var parameters = new Dictionary<string, object>
            {
                { "CartItems", _cartItems },
                { "TableNumber", _tableNumber }
            };

            // Navigation vers la page du panier
            await Shell.Current.GoToAsync("CartPage", parameters);
        }

    }

    // Classes supplémentaires pour le modèle
    public class MenuItemWithQuantity : MenuItem
    {
        public int Quantity { get; set; } = 1;
    }

    public class CartItem
    {
        public int MenuItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string SpecialRequests { get; set; }
        public decimal Total => Price * Quantity;
    }
}