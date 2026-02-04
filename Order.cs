using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S10274277E_Assignment
{ 
    public class Order
{
    public int OrderId { get; }
    public DateTime OrderDateTime { get; }
    public DateTime DeliveryDateTime { get; set; }
    public string DeliveryAddress { get; set; }
   
    public string OrderStatus { get; set; }
    public string PaymentMethod { get; set; }

    public bool IsPaid { get; set; }

    public string SpecialRequest { get; set; }
     public const double DeliveryFee = 5.00;

    private List<OrderedFoodItem> orderedFoodItems;
    public double OrderTotal { get; private set; }

    public Order(int orderId, DateTime orderDateTime, DateTime deliveryDateTime, string deliveryAddress)
    {
        OrderId = orderId;
        OrderDateTime = orderDateTime;
        DeliveryDateTime = deliveryDateTime;
        DeliveryAddress = deliveryAddress;
        OrderStatus = "Pending";
        IsPaid = false;
        orderedFoodItems = new List<OrderedFoodItem>();
    }

    public void AddOrderedFoodItem(OrderedFoodItem item)
    {
        orderedFoodItems.Add(item);
        CalculateOrderTotal();
    }

    public bool RemoveOrderedFoodItem(OrderedFoodItem item)
    {
        bool removed = orderedFoodItems.Remove(item);
        CalculateOrderTotal();
        return removed;
    }

    public double CalculateOrderTotal()
    {
        OrderTotal = 0;
        foreach (var item in orderedFoodItems)
        {
            OrderTotal += item.CalculateSubtotal();
        }
        if (orderedFoodItems.Count > 0)
            {
                OrderTotal += DeliveryFee;
            }
        return OrderTotal;
    }
        public void DisplayOrderedFoodItems()
        {
            foreach (OrderedFoodItem item in orderedFoodItems)
            {
                Console.WriteLine(item);
            }
        }
    public override string ToString()
    {
        return $"Order {OrderId} - ${OrderTotal:F2} ({OrderStatus})";
    }
 }
}

