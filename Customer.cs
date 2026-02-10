using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Student Number: S10274277E
   Student Name:   Huang Enyu (Solo)
   Partner Name:   -
*/

namespace S10274277E_Assignment
{
    public class Customer
    {
        public string EmailAddress { get; }
        public string CustomerName { get; }
        private List<Order> orders;

        public Customer(string emailAddress, string customerName)
        {
            EmailAddress = emailAddress;
            CustomerName = customerName;
            orders = new List<Order>();
        }

        public void AddOrder(Order order)
        {
            orders.Add(order);
        }

        public bool RemoveOrder(Order order)
        {
            return orders.Remove(order);
        }

        public IReadOnlyList<Order>GetOrders()
        {
            return orders.AsReadOnly();
        }

        public void DisplayAllOrders()
        {
            foreach (Order o in orders)
            {
                Console.WriteLine(o);
            }
        }

        public override string ToString()
        {
            return $"{CustomerName} ({EmailAddress})";
        }

    }
}
