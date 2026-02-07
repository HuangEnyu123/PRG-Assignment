
/* Student Number: S10274277E
   Student Name:   Huang Enyu (Solo)
   Partner Name:   -
*/

using S10274277E_Assignment;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

Dictionary<string, Restaurant> restaurants = new();
Dictionary<string, Menu> menusByRestaurant = new();
Dictionary<string, Customer> customersByEmail = new();
Dictionary<int, Order> ordersById = new();

Console.WriteLine("Welcome to the Gruberoo Food Delivery System");


string restaurantsFile = PickFirstExisting("restaurants.csv");
string foodItemsFile = PickFirstExisting("fooditems.csv", "fooditems - Copy.csv");
string customersFile = PickFirstExisting("customers.csv");
string ordersFile = PickFirstExisting("orders.csv", "orders - Copy.csv");

int rCount = LoadRestaurants(restaurants, menusByRestaurant, restaurantsFile);
int fCount = LoadFoodItems(restaurants, menusByRestaurant, foodItemsFile);

int cCount = LoadCustomers(customersByEmail, customersFile);
int oCount = LoadOrders(ordersById, restaurants, menusByRestaurant, customersByEmail, ordersFile);

Console.WriteLine($"{rCount} restaurants loaded!");
Console.WriteLine($"{fCount} food items loaded!");
Console.WriteLine($"{cCount} customers loaded!");
Console.WriteLine($"{oCount} orders loaded!");

while (true)
{
    Console.WriteLine("\n===== Gruberoo Food Delivery System =====");
    Console.WriteLine("1. List all restaurants and menu items");
    Console.WriteLine("2. List all orders");
    Console.WriteLine("3. Create a new order");
    Console.WriteLine("0. Exit");
    Console.Write("Enter your choice: ");

    int choice = ReadIntRange(0, 3);

    if (choice == 0)
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    switch (choice)
    {

        case 1:
            ListRestaurantsAndMenuItems(restaurants);
            break;

        case 2:
            ListAllOrders(ordersById, customersByEmail, restaurants);
            break;

        case 3:
            CreateNewOrder(ordersById, restaurants, menusByRestaurant, customersByEmail, ordersFile);
            break;
    }
}

int LoadRestaurants(Dictionary<string, Restaurant> restaurants,
                    Dictionary<string, Menu> menusByRestaurant,
                    string filePath)
{
    if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
    {
        Console.WriteLine("ERROR: restaurants.csv not found.");
        return 0;
    }

    int loaded = 0;

    try
    {
        using StreamReader sr = new(filePath);
        _ = sr.ReadLine(); // header

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] p = SplitCsvLine(line);
            if (p.Length < 3) continue;

            string restId = p[0].Trim();
            string restName = p[1].Trim();
            string restEmail = p[2].Trim();

            if (string.IsNullOrWhiteSpace(restId) ||
                string.IsNullOrWhiteSpace(restName) ||
                string.IsNullOrWhiteSpace(restEmail))
                continue;

            if (restaurants.ContainsKey(restId)) continue;

            Restaurant r = new Restaurant(restId, restName, restEmail);
            restaurants[restId] = r;

            // Default menu per restaurant
            Menu m = new Menu($"M_{restId}", "Main Menu");
            r.AddMenu(m);
            menusByRestaurant[restId] = m;

            loaded++;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR reading restaurants file: {ex.Message}");
    }

    return loaded;
}

int LoadFoodItems(Dictionary<string, Restaurant> restaurants,
                  Dictionary<string, Menu> menusByRestaurant,
                  string filePath)
{
    if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
    {
        Console.WriteLine("ERROR: fooditems file not found.");
        return 0;
    }

    int loaded = 0;

    try
    {
        using StreamReader sr = new(filePath);
        _ = sr.ReadLine(); // header

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] p = SplitCsvLine(line);
            if (p.Length < 4) continue;

            string restId = p[0].Trim();
            string itemName = p[1].Trim();
            string itemDesc = p[2].Trim();
            string priceStr = p[3].Trim();

            if (!restaurants.ContainsKey(restId) || !menusByRestaurant.ContainsKey(restId))
                continue;

            if (string.IsNullOrWhiteSpace(itemName) || string.IsNullOrWhiteSpace(itemDesc))
                continue;

            if (!double.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double price) || price < 0)
                continue;

            menusByRestaurant[restId].AddFoodItem(new FoodItem(itemName, itemDesc, price));
            loaded++;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR reading food items file: {ex.Message}");
    }

    return loaded;
}

int LoadCustomers(Dictionary<string, Customer> customersByEmail, string filePath)
{
    if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
    {
        Console.WriteLine("ERROR: customers.csv not found.");
        return 0;
    }

    int loaded = 0;

    try
    {
        using StreamReader sr = new(filePath);
        _ = sr.ReadLine(); // header

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] p = SplitCsvLine(line);
            if (p.Length < 2) continue;

            string name = p[0].Trim();
            string email = p[1].Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email)) continue;
            if (!IsValidEmail(email)) continue;

            if (customersByEmail.ContainsKey(email)) continue;

            customersByEmail[email] = new Customer(email, name);
            loaded++;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR reading customers file: {ex.Message}");
    }

    return loaded;
}

int LoadOrders(Dictionary<int, Order> ordersById,
               Dictionary<string, Restaurant> restaurants,
               Dictionary<string, Menu> menusByRestaurant,
               Dictionary<string, Customer> customersByEmail,
               string filePath)
{
    if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
    {
        Console.WriteLine("ERROR: orders file not found.");
        return 0;
    }

    int loaded = 0;

    try
    {
        using StreamReader sr = new(filePath);
        _ = sr.ReadLine(); // header:
      
        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] p = SplitCsvLine(line);
            if (p.Length < 10) continue;

            if (!int.TryParse(p[0].Trim(), out int orderId)) continue;

            string custEmail = p[1].Trim();
            string restId = p[2].Trim();
            string deliveryDateStr = p[3].Trim();
            string deliveryTimeStr = p[4].Trim(); 
            string address = p[5].Trim();
            string createdStr = p[6].Trim();     
            string totalStr = p[7].Trim();
            string status = p[8].Trim();
            string itemsStr = p[9].Trim();

            if (ordersById.ContainsKey(orderId)) continue;
            if (!customersByEmail.ContainsKey(custEmail)) continue;
            if (!restaurants.ContainsKey(restId) || !menusByRestaurant.ContainsKey(restId)) continue;
            if (string.IsNullOrWhiteSpace(address)) continue;

            if (!DateTime.TryParseExact(deliveryDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dDate))
                continue;

            if (!DateTime.TryParseExact(deliveryTimeStr, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dTime))
                continue;

            DateTime deliveryDt = dDate.Date.Add(dTime.TimeOfDay);

            if (!DateTime.TryParseExact(createdStr, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime createdDt))
                createdDt = DateTime.Now;

            Order o = new Order(orderId, custEmail, restId, createdDt, deliveryDt, address);
            o.OrderStatus = string.IsNullOrWhiteSpace(status) ? "Pending" : status;

            BuildOrderedItemsFromString(o, menusByRestaurant[restId], itemsStr);

            if (o.GetOrderedFoodItems().Count == 0 &&
                double.TryParse(totalStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double fileTotal) &&
                fileTotal >= 0)
            {
                
            }

            restaurants[restId].EnqueueOrder(o);
            customersByEmail[custEmail].AddOrder(o);

            ordersById[orderId] = o;
            loaded++;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR reading orders file: {ex.Message}");
    }

    return loaded;
}