using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
/* Student Number: S10274277E
   Student Name:   Huang Enyu (Solo)
   Partner Name:   -
*/

using S10274277E_Assignment;

Dictionary<string, Restaurant> restaurants = new();
Dictionary<string, Menu> menus = new();

Console.WriteLine("Welcome to the Gruberoo Food Delivery System");

string restaurantsFile = "restaurants.csv";
string foodItemsFile = "fooditems - Copy.csv";

int rCount = LoadRestaurants(restaurants, menus, restaurantsFile);
int fCount = LoadFoodItems(restaurants, menus, foodItemsFile);

Console.WriteLine($"{rCount} restaurants loaded!");
Console.WriteLine($"{fCount} food items loaded!");

while (true)
{
    Console.WriteLine("\n===== Gruberoo Food Delivery System =====");
    Console.WriteLine("1. List all restaurants and menu items");
    Console.WriteLine("0. Exit");
    Console.Write("Enter your choice: ");

    string input = Console.ReadLine();
    if (!int.TryParse(input, out int choice))
    {
        Console.WriteLine("Invalid input. Please enter a number.");
        continue;
    }

    if (choice == 0)
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    if (choice == 1)
    {
        ListRestaurantsAndMenuItems(restaurants);
    }
    else
    {
        Console.WriteLine("Invalid option.");
    }
}

int LoadRestaurants(Dictionary<string, Restaurant> restaurants,
                    Dictionary<string, Menu> menus,
                    string filePath)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"ERROR: '{filePath}' not found.");
        return 0;
    }

    int loaded = 0;

    try
    {
        using StreamReader sr = new(filePath);
        string header = sr.ReadLine();
        if (string.IsNullOrWhiteSpace(header))
        {
            Console.WriteLine($"ERROR: '{filePath}' is empty.");
            return 0;
        }

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] p = line.Split(',');
            if (p.Length < 3) continue;

            string restId = p[0].Trim();
            string restName = p[1].Trim();
            string restEmail = p[2].Trim();

            if (string.IsNullOrWhiteSpace(restId) ||
                string.IsNullOrWhiteSpace(restName) ||
                string.IsNullOrWhiteSpace(restEmail)) continue;

            if (restaurants.ContainsKey(restId)) continue;

            Restaurant r = new Restaurant(restId, restName, restEmail);
            restaurants.Add(restId, r);

            Menu m = new Menu($"M_{restId}", "Main Menu");
            r.AddMenu(m);
            menus.Add(restId, m);

            loaded++;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR reading '{filePath}': {ex.Message}");
    }

    return loaded;
}

int LoadFoodItems(Dictionary<string, Restaurant> restaurants,
                  Dictionary<string, Menu> menus,
                  string filePath)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"ERROR: '{filePath}' not found.");
        return 0;
    }

    int loaded = 0;

    try
    {
        using StreamReader sr = new(filePath);
        string header = sr.ReadLine();
        if (string.IsNullOrWhiteSpace(header))
        {
            Console.WriteLine($"ERROR: '{filePath}' is empty.");
            return 0;
        }

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] p = line.Split(',');
            if (p.Length < 4) continue;

            string restId = p[0].Trim();
            string itemName = p[1].Trim();
            string itemDesc = p[2].Trim();
            string priceStr = p[3].Trim();

            if (!restaurants.ContainsKey(restId) || !menus.ContainsKey(restId)) continue;
            if (string.IsNullOrWhiteSpace(itemName) || string.IsNullOrWhiteSpace(itemDesc)) continue;

            if (!double.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double price) || price < 0)
                continue;

            FoodItem fi = new FoodItem(itemName, itemDesc, price);
            menus[restId].AddFoodItem(fi);
            loaded++;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR reading '{filePath}': {ex.Message}");
    }

    return loaded;
}

void ListRestaurantsAndMenuItems(Dictionary<string, Restaurant> restaurants)
{
    if (restaurants.Count == 0)
    {
        Console.WriteLine("No restaurants loaded.");
        return;
    }

    foreach (Restaurant r in restaurants.Values)
    {
        Console.WriteLine($"\n{r}");
        r.DisplayMenu();
    }
}

