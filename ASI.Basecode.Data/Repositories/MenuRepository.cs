using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class MenuRepository : BaseRepository, IMenuRepository
    {
        public MenuRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<MenuItem> GetMenuItems()
        {
            return this.GetDbSet<MenuItem>();
        }

        public MenuItem GetMenuItemById(int id)
        {
            return this.GetDbSet<MenuItem>().FirstOrDefault(x => x.Id == id);
        }

        public void AddMenuItem(MenuItem menuItem)
        {
            this.GetDbSet<MenuItem>().Add(menuItem);
            UnitOfWork.SaveChanges();
        }

        public void UpdateMenuItem(MenuItem menuItem)
        {
            this.GetDbSet<MenuItem>().Update(menuItem);
            UnitOfWork.SaveChanges();
        }

        public void DeleteMenuItem(int id)
        {
            var menuItem = GetMenuItemById(id);
            if (menuItem != null)
            {
                // Permanently delete the menu item from the database
                this.GetDbSet<MenuItem>().Remove(menuItem);
                UnitOfWork.SaveChanges();
            }
        }

        public bool MenuItemExists(int id)
        {
            return this.GetDbSet<MenuItem>().Any(x => x.Id == id && x.IsActive);
        }

        public bool MenuItemExists(string name, int? excludeId = null)
        {
            var query = this.GetDbSet<MenuItem>().Where(x => x.Name.ToLower() == name.ToLower() && x.IsActive);
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }
            return query.Any();
        }

        public bool HasSufficientStock(int menuItemId, int requestedQuantity)
        {
            var menuItem = GetMenuItemById(menuItemId);
            return menuItem != null && menuItem.IsActive && menuItem.Stock >= requestedQuantity;
        }

        public void DecrementStock(int menuItemId, int quantity)
        {
            var menuItem = GetMenuItemById(menuItemId);
            if (menuItem != null && menuItem.Stock >= quantity)
            {
                menuItem.Stock -= quantity;
                menuItem.UpdatedTime = System.DateTime.Now;
                menuItem.UpdatedBy = System.Environment.UserName;
                UpdateMenuItem(menuItem);
            }
        }

        public void IncrementStock(int menuItemId, int quantity)
        {
            var menuItem = GetMenuItemById(menuItemId);
            if (menuItem != null)
            {
                menuItem.Stock += quantity;
                menuItem.UpdatedTime = System.DateTime.Now;
                menuItem.UpdatedBy = System.Environment.UserName;
                UpdateMenuItem(menuItem);
            }
        }
    }
}