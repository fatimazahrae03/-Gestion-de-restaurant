using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp2.Models;
using MauiApp2.Services;
using MauiApp2.Views;
using MenuItem = MauiApp2.Models.MenuItem;

namespace MauiApp2.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly MenuService _menuService;
        private Category _selectedCategory;
        private MenuItem _selectedMenuItem;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<MenuItem> _menuItems;

        public MenuViewModel(MenuService menuService)
        {
            _menuService = menuService;
            LoadCategoriesCommand = new Command(async () => await LoadCategoriesAsync());
            AddCategoryCommand = new Command(async () => await AddCategoryAsync());
            EditCategoryCommand = new Command<Category>(async (category) => await EditCategoryAsync(category));
            DeleteCategoryCommand = new Command<Category>(async (category) => await DeleteCategoryAsync(category));
            AddMenuItemCommand = new Command(async () => await AddMenuItemAsync());
            EditMenuItemCommand = new Command<MenuItem>(async (menuItem) => await EditMenuItemAsync(menuItem));
            DeleteMenuItemCommand = new Command<MenuItem>(async (menuItem) => await DeleteMenuItemAsync(menuItem));
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<MenuItem> MenuItems
        {
            get => _menuItems;
            set => SetProperty(ref _menuItems, value);
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value) && _selectedCategory != null)
                {
                    LoadMenuItemsAsync(_selectedCategory.Id);
                }
            }
        }

        public MenuItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set => SetProperty(ref _selectedMenuItem, value);
        }

        public ICommand LoadCategoriesCommand { get; }
        public ICommand AddCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand AddMenuItemCommand { get; }
        public ICommand EditMenuItemCommand { get; }
        public ICommand DeleteMenuItemCommand { get; }

        public async Task LoadCategoriesAsync()
        {
            try
            {
                IsBusy = true;
                Categories = await _menuService.GetCategoriesAsync();
                
                // If there are categories, select the first one to show its items
                if (Categories.Count > 0)
                {
                    SelectedCategory = Categories[0];
                }
                else
                {
                    // Empty the menu items if no categories
                    MenuItems = new ObservableCollection<MenuItem>();
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (log, display message, etc.)
                System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load categories.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadMenuItemsAsync(int categoryId)
        {
            try
            {
                IsBusy = true;
                MenuItems = await _menuService.GetMenuItemsByCategoryAsync(categoryId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading menu items: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Failed to load menu items.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddCategoryAsync()
        {
            var category = new Category { Name = "", Description = "" };
            var page = new CategoryDetailPage(category);
            page.CategorySaved += async (s, e) =>
            {
                try
                {
                    await _menuService.AddCategoryAsync(category);
                    await LoadCategoriesAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors de la sauvegarde: {ex}");
                    await Shell.Current.DisplayAlert("Erreur", $"Impossible de sauvegarder: {ex.Message}", "OK");
                }
            };
            await Shell.Current.Navigation.PushModalAsync(page);
        }

        private async Task EditCategoryAsync(Category category)
        {
            if (category == null) return;
            
            var categoryCopy = new Category
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
            
            var page = new CategoryDetailPage(categoryCopy);
            page.CategorySaved += async (s, e) =>
            {
                try
                {
                    await _menuService.AddCategoryAsync(category);
                    await LoadCategoriesAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors de la sauvegarde: {ex}");
                    await Shell.Current.DisplayAlert("Erreur", $"Impossible de sauvegarder: {ex.Message}", "OK");
                }
            };
            await Shell.Current.Navigation.PushModalAsync(page);
        }

        private async Task DeleteCategoryAsync(Category category)
        {
            if (category == null) return;

            bool answer = await Shell.Current.DisplayAlert(
                "Confirmation", 
                $"Êtes-vous sûr de vouloir supprimer la catégorie '{category.Name}' et tous ses plats ?", 
                "Oui", "Non");
                
            if (answer)
            {
                await _menuService.DeleteCategoryAsync(category.Id);
                await LoadCategoriesAsync();
            }
        }

        private async Task AddMenuItemAsync()
        {
            if (SelectedCategory == null)
            {
                await Shell.Current.DisplayAlert("Info", "Veuillez d'abord sélectionner une catégorie.", "OK");
                return;
            }

            var menuItem = new MenuItem { 
                CategoryId = SelectedCategory.Id,
                IsAvailable = true 
            };
            
            var page = new MenuItemDetailPage(menuItem, Categories);
            page.MenuItemSaved += async (s, e) =>
            {
                await _menuService.AddMenuItemAsync(menuItem);
                await LoadMenuItemsAsync(SelectedCategory.Id);
            };
            await Shell.Current.Navigation.PushModalAsync(page);
        }

        private async Task EditMenuItemAsync(MenuItem menuItem)
        {
            if (menuItem == null) return;
            
            var menuItemCopy = new MenuItem
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                CategoryId = menuItem.CategoryId,
                IsAvailable = menuItem.IsAvailable,
                ImagePath = menuItem.ImagePath
            };
            
            var page = new MenuItemDetailPage(menuItemCopy, Categories);
            page.MenuItemSaved += async (s, e) =>
            {
                await _menuService.UpdateMenuItemAsync(menuItemCopy);
                await LoadMenuItemsAsync(SelectedCategory.Id);
            };
            await Shell.Current.Navigation.PushModalAsync(page);
        }

        private async Task DeleteMenuItemAsync(MenuItem menuItem)
        {
            if (menuItem == null) return;

            bool answer = await Shell.Current.DisplayAlert(
                "Confirmation", 
                $"Êtes-vous sûr de vouloir supprimer le plat '{menuItem.Name}' ?", 
                "Oui", "Non");
                
            if (answer)
            {
                await _menuService.DeleteMenuItemAsync(menuItem.Id);
                await LoadMenuItemsAsync(SelectedCategory.Id);
            }
        }
    }
}