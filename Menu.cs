using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S10274277E_Assignment
{

    public class Menu
    {
        public string MenuId { get; }
        public string MenuName { get; }
        private List<FoodItem> foodItems;

        public Menu(string menuId, string menuName)
        {
            MenuId = menuId;
            MenuName = menuName;
            foodItems = new List<FoodItem>();
        }

        public void AddFoodItem(FoodItem foodItem)
        {
            foodItems.Add(foodItem);
        }

        public bool RemoveFoodItem(FoodItem foodItem)
        {
            return foodItems.Remove(foodItem);
        }

        public List<FoodItem> GetFoodItems()
        {
            return foodItems;
        }

        public void DisplayFoodItems()
        {
            foreach (FoodItem item in foodItems)
            {
                Console.WriteLine(item);
            }
        }

        public override string ToString()
        {
            return $"Menu: {MenuName} ({MenuId})";
        }

    }
}
