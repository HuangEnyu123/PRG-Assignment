using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S10274277E_Assignment
{

    public class OrderedFoodItem
    {
        public FoodItem FoodItem { get; }
        public int QtyOrdered { get; }
        public double SubTotal { get; private set; }

        public OrderedFoodItem(FoodItem foodItem, int qtyOrdered)
        {
            FoodItem = foodItem;
            QtyOrdered = qtyOrdered;
            CalculateSubtotal();
        }

        public double CalculateSubtotal()
        {
            SubTotal = FoodItem.ItemPrice * QtyOrdered;
            return SubTotal;
        }

        public override string ToString()
        {
            return $"{FoodItem.ItemName} - {QtyOrdered} (${SubTotal:F2})";
        }

    }
}