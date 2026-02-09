using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S10274277E_Assignment
{
    public class Restaurant
    {
        public string RestaurantId { get; }
        public string RestaurantName { get; }
        public string RestaurantEmail { get; }

        private List<Menu> menus;
        private List<SpecialOffer> specialOffers;
        private Queue<Order> orderQueue;

        public Restaurant(string restaurantId, string restaurantName, string restaurantEmail)
        {
            RestaurantId = restaurantId;
            RestaurantName = restaurantName;
            RestaurantEmail = restaurantEmail;
            menus = new List<Menu>();
            specialOffers = new List<SpecialOffer>();
            orderQueue = new Queue<Order>();
        }

        public void AddMenu(Menu menu)
        {
            menus.Add(menu);
        }

        public bool RemoveMenu(Menu menu)
        {
            return menus.Remove(menu);
        }

        public void AddSpecialOffer(SpecialOffer offer)
        {
            specialOffers.Add(offer);
        }

        public IReadOnlyList<SpecialOffer> GetSpecialOffers()
        {
            return specialOffers.AsReadOnly();
        }

        public bool RemoveSpecialOffer(SpecialOffer offer)
        {
            return specialOffers.Remove(offer);
        }
        public void EnqueueOrder(Order order)
        {
            orderQueue.Enqueue(order);
        }

        public Queue<Order> GetOrderQueue()
        {
            return orderQueue;
        }

        public void DisplayMenu()
        {
            foreach (Menu m in menus)
            {
                Console.WriteLine(m);
                m.DisplayFoodItems();
            }
        }

        public void DisplayOrders()
        {
            foreach (Order o in orderQueue)
            {
                Console.WriteLine(o);
            }
        }

        public void DisplaySpecialOffers()
        {
            foreach (SpecialOffer offer in specialOffers)
            {
                Console.WriteLine(offer);
            }
        }
        public override string ToString()
        {
            return $"{RestaurantName} ({RestaurantId})";
        }
    }
}