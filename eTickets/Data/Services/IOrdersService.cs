using eTickets.Models;

namespace eTickets.Data.Services
{
    public interface IOrdersService 
    {
        //new order and orderItem
        Task StoreOrderAsync(List<ShoppingCartItem> items, string userId, string userEmailAddress);

        //Showing orders for admin and users
        Task<List<Order>> GetOrdersByUserIdAndRoleAsync(string userId, string userRole);
    }
}
