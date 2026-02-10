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
    public class Order
    {
        public int OrderId { get; }
        public DateTime OrderDateTime { get; }
        public DateTime DeliveryDateTime { get; set; }
        public string DeliveryAddress { get; set; }

        public string CustomerEmail { get; }
        public string RestaurantId { get; }

        public string OrderStatus { get; set; }
        public string PaymentMethod { get; set; }
        public bool IsPaid { get; set; }
        public string SpecialRequest { get; set; }

        public const double DeliveryFee = 5.00;

        private readonly List<OrderedFoodItem> orderedFoodItems;
        public double OrderTotal { get; private set; }

        public string AppliedOfferCode { get; private set; } = "";
        public double DiscountPercentApplied { get; private set; } = 0;
        public bool FreeDeliveryApplied { get; private set; } = false;


        public Order(int orderId, string customerEmail, string restaurantId,
                     DateTime orderDateTime, DateTime deliveryDateTime, string deliveryAddress)
        {
            OrderId = orderId;
            CustomerEmail = customerEmail;
            RestaurantId = restaurantId;

            OrderDateTime = orderDateTime;
            DeliveryDateTime = deliveryDateTime;
            DeliveryAddress = deliveryAddress;

            OrderStatus = "Pending";
            PaymentMethod = "";
            SpecialRequest = "";
            IsPaid = false;

            orderedFoodItems = new List<OrderedFoodItem>();
            CalculateOrderTotal();
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

        public IReadOnlyList<OrderedFoodItem> GetOrderedFoodItems()
        {
            return orderedFoodItems.AsReadOnly();
        }

        public double CalculateOrderTotal()
        {
            double itemsSubTotal = 0;

            foreach (var item in orderedFoodItems)
                itemsSubTotal += item.CalculateSubtotal();
      
            double discountedItems = itemsSubTotal;
            if (DiscountPercentApplied > 0)
                discountedItems = itemsSubTotal * (1.0 - DiscountPercentApplied / 100.0);

            double deliveryFee = (orderedFoodItems.Count > 0 && !FreeDeliveryApplied)
                ? DeliveryFee
                : 0.0;

            OrderTotal = Math.Round(discountedItems + deliveryFee, 2);
            return OrderTotal;
        }

        public void ApplySpecialOffer(SpecialOffer offer)
        {
 
            if (offer == null)
            {
                AppliedOfferCode = "";
                DiscountPercentApplied = 0;
                FreeDeliveryApplied = false;
                CalculateOrderTotal();
                return;
            }

            AppliedOfferCode = offer.OfferCode;
            DiscountPercentApplied = offer.Discount;
            FreeDeliveryApplied = false;

            double itemsSubTotal = orderedFoodItems.Sum(x => x.CalculateSubtotal());

            if (offer.Discount <= 0 &&
                offer.OfferDesc != null &&
                offer.OfferDesc.Contains("Free Delivery", StringComparison.OrdinalIgnoreCase) &&
                itemsSubTotal >= 30.0)
            {
                FreeDeliveryApplied = true;
            }

            CalculateOrderTotal();
        }

        public void SetTotalFromFile(double total)
        {
            if (total < 0) total = 0;
            OrderTotal = total;
        }


        public void DisplayOrderedFoodItems()
        {
            foreach (var item in orderedFoodItems)
                Console.WriteLine(item);
        }

        public override string ToString()
        {
            return $"Order {OrderId} - ${OrderTotal:F2} ({OrderStatus})";
        }
    }
}


