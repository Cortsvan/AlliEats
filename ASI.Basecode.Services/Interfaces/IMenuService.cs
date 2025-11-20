using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IMenuService
    {
        IEnumerable<MenuItemViewModel> GetAllMenuItems();
        IEnumerable<MenuItemViewModel> GetActiveMenuItems();
        MenuItemViewModel GetMenuItemById(int id);
        void AddMenuItem(MenuItemViewModel model);
        void UpdateMenuItem(MenuItemViewModel model);
        void DeleteMenuItem(int id);
        bool MenuItemExists(int id);
        bool MenuItemNameExists(string name, int? excludeId = null);
        bool HasSufficientStock(int menuItemId, int requestedQuantity);
        void DecrementStock(int menuItemId, int quantity);
        void IncrementStock(int menuItemId, int quantity);

        // Dashboard methods
        IEnumerable<MenuItemViewModel> GetFeaturedMenuItems(int count = 6);
        IEnumerable<string> GetTopCategories(int count = 3);

        // search methods
        MenuSearchResult SearchMenuItems(string query, int limit = 5);
        (bool IsValid, string Message) ValidateSearchQuery(string query);
    }

    // new search result class
    public class MenuSearchResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalResults { get; set; }
        public List<MenuItemSearchViewModel> Items { get; set; } = new List<MenuItemSearchViewModel>();
    }

    
}