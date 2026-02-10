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
        public class FoodItem
        {
            public string ItemName { get; }
            public string ItemDesc { get; }
            public double ItemPrice { get; }

       
            public string Customise { get; private set; } = "";

            public FoodItem(string itemName, string itemDesc, double itemPrice)
            {
                ItemName = itemName;
                ItemDesc = itemDesc;
                ItemPrice = itemPrice;
            }

        
            public void SetCustomise(string customise)
            {
                customise ??= "";
                customise = customise.Trim();

                if (customise.Length > 60)
                    customise = customise.Substring(0, 60);  

                Customise = customise;
            }

            public FoodItem CopyForOrder(string customise)
            {
                FoodItem copy = new FoodItem(ItemName, ItemDesc, ItemPrice);
                copy.SetCustomise(customise);
                return copy;
            }

            public override string ToString()
            {
                if (!string.IsNullOrWhiteSpace(Customise))
                    return $"{ItemName}: {ItemDesc} - ${ItemPrice:F2} (Special Request: {Customise})";

                return $"{ItemName}: {ItemDesc} - ${ItemPrice:F2}";
            }
        }
    }
