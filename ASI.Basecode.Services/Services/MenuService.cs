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
        private readonly IReviewRepository _reviewRepository;

        public MenuService(IMenuRepository repository, IMapper mapper, IReviewRepository reviewRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _reviewRepository = reviewRepository;
        }

        public IEnumerable<MenuItemViewModel> GetAllMenuItems()
        {
            var menuItems = _repository.GetMenuItems().OrderBy(x => x.Name);
            var menuItemViewModels = _mapper.Map<IEnumerable<MenuItemViewModel>>(menuItems).ToList();
            
            // Calculate ratings for each menu item
            foreach (var item in menuItemViewModels)
            {
                CalculateMenuItemRating(item);
            }
            
            return menuItemViewModels;
        }

        public IEnumerable<MenuItemViewModel> GetActiveMenuItems()
        {
            var menuItems = _repository.GetMenuItems().Where(x => x.IsActive).OrderBy(x => x.Name);
            var menuItemViewModels = _mapper.Map<IEnumerable<MenuItemViewModel>>(menuItems).ToList();
            
            // Calculate ratings for each menu item
            foreach (var item in menuItemViewModels)
            {
                CalculateMenuItemRating(item);
            }
            
            return menuItemViewModels;
        }

        public MenuItemViewModel GetMenuItemById(int id)
        {
            var menuItem = _repository.GetMenuItemById(id);
            var menuItemViewModel = _mapper.Map<MenuItemViewModel>(menuItem);
            
            // Calculate rating for the menu item
            if (menuItemViewModel != null)
            {
                CalculateMenuItemRating(menuItemViewModel);
            }
            
            return menuItemViewModel;
        }

        private void CalculateMenuItemRating(MenuItemViewModel menuItem)
        {
            // Get all reviews
            var allReviews = _reviewRepository.GetAllReviews();
            
            // Filter reviews that contain this menu item
            var relevantReviews = allReviews
                .Where(r => r.Order != null && r.Order.OrderItems != null && 
                            r.Order.OrderItems.Any(oi => oi.MenuItemId == menuItem.Id))
                .ToList();
            
            if (relevantReviews.Any())
            {
                menuItem.AverageRating = relevantReviews.Average(r => r.Rating);
                menuItem.ReviewCount = relevantReviews.Count();
            }
            else
            {
                menuItem.AverageRating = 0;
                menuItem.ReviewCount = 0;
            }
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
        
        public bool HasSufficientStock(int menuItemId, int requestedQuantity)
        {
            return _repository.HasSufficientStock(menuItemId, requestedQuantity);
        }

        public void DecrementStock(int menuItemId, int quantity)
        {
            _repository.DecrementStock(menuItemId, quantity);
        }

        public void IncrementStock(int menuItemId, int quantity)
        {
            _repository.IncrementStock(menuItemId, quantity);
        }
    }
}