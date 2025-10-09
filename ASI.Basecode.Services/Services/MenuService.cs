using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _repository;
        private readonly IMapper _mapper;

        public MenuService(IMenuRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public IEnumerable<MenuItemViewModel> GetAllMenuItems()
        {
            var menuItems = _repository.GetMenuItems().OrderBy(x => x.Name);
            return _mapper.Map<IEnumerable<MenuItemViewModel>>(menuItems);
        }

        public IEnumerable<MenuItemViewModel> GetActiveMenuItems()
        {
            var menuItems = _repository.GetMenuItems().Where(x => x.IsActive).OrderBy(x => x.Name);
            return _mapper.Map<IEnumerable<MenuItemViewModel>>(menuItems);
        }

        public MenuItemViewModel GetMenuItemById(int id)
        {
            var menuItem = _repository.GetMenuItemById(id);
            return _mapper.Map<MenuItemViewModel>(menuItem);
        }

        public void AddMenuItem(MenuItemViewModel model)
        {
            if (MenuItemNameExists(model.Name))
            {
                throw new InvalidOperationException("A menu item with this name already exists.");
            }

            var menuItem = _mapper.Map<MenuItem>(model);
            menuItem.CreatedTime = DateTime.Now;
            menuItem.UpdatedTime = DateTime.Now;
            menuItem.CreatedBy = System.Environment.UserName;
            menuItem.UpdatedBy = System.Environment.UserName;

            _repository.AddMenuItem(menuItem);
        }

        public void UpdateMenuItem(MenuItemViewModel model)
        {
            if (MenuItemNameExists(model.Name, model.Id))
            {
                throw new InvalidOperationException("A menu item with this name already exists.");
            }

            var existingMenuItem = _repository.GetMenuItemById(model.Id);
            if (existingMenuItem == null)
            {
                throw new InvalidOperationException("Menu item not found.");
            }

            _mapper.Map(model, existingMenuItem);
            existingMenuItem.UpdatedTime = DateTime.Now;
            existingMenuItem.UpdatedBy = System.Environment.UserName;

            _repository.UpdateMenuItem(existingMenuItem);
        }

        public void DeleteMenuItem(int id)
        {
            _repository.DeleteMenuItem(id);
        }

        public bool MenuItemExists(int id)
        {
            return _repository.MenuItemExists(id);
        }

        public bool MenuItemNameExists(string name, int? excludeId = null)
        {
            return _repository.MenuItemExists(name, excludeId);
        }
    }
}