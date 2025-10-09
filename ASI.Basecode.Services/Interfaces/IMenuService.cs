using ASI.Basecode.Data.Models;
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
    }
}