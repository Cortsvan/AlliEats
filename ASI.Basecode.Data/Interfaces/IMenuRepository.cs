using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IMenuRepository
    {
        IQueryable<MenuItem> GetMenuItems();
        MenuItem GetMenuItemById(int id);
        void AddMenuItem(MenuItem menuItem);
        void UpdateMenuItem(MenuItem menuItem);
        void DeleteMenuItem(int id);
        bool MenuItemExists(int id);
        bool MenuItemExists(string name, int? excludeId = null);
    }
}