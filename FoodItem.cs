using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S10274277E_Assignment
{
    public class FoodItem
    {
        public string ItemName { get; }
        public string ItemDesc { get; }
        public double ItemPrice { get; }
        public string Customise { get; set; }

        public FoodItem(string itemName, string itemDesc, double itemPrice)
        {
            ItemName = itemName;
            ItemDesc = itemDesc;
            ItemPrice = itemPrice;
            Customise = string.Empty;
        }

        public override string ToString()
        {
            return $"{ItemName}: {ItemDesc} - ${ItemPrice:F2}";
        }

    }
}
