
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
        _ = sr.ReadLine(); 

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
        _ = sr.ReadLine(); 

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
        _ = sr.ReadLine(); 

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
        _ = sr.ReadLine(); 
      
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

void ListRestaurantsAndMenuItems(Dictionary<string, Restaurant> restaurants)
{
    if (restaurants.Count == 0)
    {
        Console.WriteLine("No restaurants loaded.");
        return;
    }

    Console.WriteLine("\nAll Restaurants and Menu Items");
    Console.WriteLine("==============================");

    foreach (Restaurant r in restaurants.Values.OrderBy(x => x.RestaurantId))
    {
        Console.WriteLine($"\nRestaurant: {r.RestaurantName} ({r.RestaurantId})");
        r.DisplayMenu();
    }
}

void ListAllOrders(Dictionary<int, Order> ordersById,
                   Dictionary<string, Customer> customersByEmail,
                   Dictionary<string, Restaurant> restaurants)
{
    if (ordersById.Count == 0)
    {
        Console.WriteLine("No orders loaded.");
        return;
    }

    Console.WriteLine("\nAll Orders");
    Console.WriteLine("==========");

    Console.WriteLine($"{"Order ID",-8} {"Customer",-18} {"Restaurant",-18} {"Delivery Date/Time",-18} {"Amount",-10} {"Status",-12}");
    Console.WriteLine(new string('-', 90));

    foreach (Order o in ordersById.Values.OrderBy(x => x.OrderId))
    {
        string custName = customersByEmail.TryGetValue(o.CustomerEmail, out var c) ? c.CustomerName : o.CustomerEmail;
        string restName = restaurants.TryGetValue(o.RestaurantId, out var r) ? r.RestaurantName : o.RestaurantId;

        Console.WriteLine($"{o.OrderId,-8} {TrimTo(custName, 18),-18} {TrimTo(restName, 18),-18} {o.DeliveryDateTime:dd/MM/yyyy HH:mm,-18} ${o.OrderTotal,-9:F2} {o.OrderStatus,-12}");
    }
}

void CreateNewOrder(Dictionary<int, Order> ordersById,
                    Dictionary<string, Restaurant> restaurants,
                    Dictionary<string, Menu> menusByRestaurant,
                    Dictionary<string, Customer> customersByEmail,
                    string ordersFilePath)
{
    Console.WriteLine("\nCreate New Order");
    Console.WriteLine("================");

    string custEmail;
    while (true)
    {
        custEmail = ReadNonEmpty("Enter Customer Email: ");
        if (!IsValidEmail(custEmail))
        {
            Console.WriteLine("Invalid email format.");
            continue;
        }
        if (!customersByEmail.ContainsKey(custEmail))
        {
            Console.WriteLine("Customer not found.");
            continue;
        }
        break;
    }

    string restId;
    while (true)
    {
        restId = ReadNonEmpty("Enter Restaurant ID: ").ToUpperInvariant();
        if (!restaurants.ContainsKey(restId))
        {
            Console.WriteLine("Restaurant not found.");
            continue;
        }
        if (!menusByRestaurant.ContainsKey(restId))
        {
            Console.WriteLine("Menu not found for this restaurant.");
            return;
        }
        break;
    }

    DateTime deliveryDate;
    while (true)
    {
        string d = ReadNonEmpty("Enter Delivery Date (dd/mm/yyyy): ");
        if (!DateTime.TryParseExact(d, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out deliveryDate))
        {
            Console.WriteLine("Invalid date. Use dd/mm/yyyy.");
            continue;
        }
        break;
    }

    DateTime deliveryTime;
    while (true)
    {
        string t = ReadNonEmpty("Enter Delivery Time (hh:mm): ");
        if (!DateTime.TryParseExact(t, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out deliveryTime))
        {
            Console.WriteLine("Invalid time. Use hh:mm (24-hour).");
            continue;
        }
        break;
    }

    DateTime deliveryDt = deliveryDate.Date.Add(deliveryTime.TimeOfDay);
    if (deliveryDt < DateTime.Now)
    {
        Console.WriteLine("Delivery date/time cannot be in the past. Order cancelled.");
        return;
    }

    string address = ReadNonEmpty("Enter Delivery Address: ");

    int newOrderId = (ordersById.Count == 0) ? 1001 : ordersById.Keys.Max() + 1;

    Order order = new Order(newOrderId, custEmail, restId, DateTime.Now, deliveryDt, address);

    Menu menu = menusByRestaurant[restId];
    List<FoodItem> items = menu.GetFoodItems().ToList();

    if (items.Count == 0)
    {
        Console.WriteLine("This restaurant has no food items. Order cancelled.");
        return;
    }

    Console.WriteLine("Available Food Items:");
    for (int i = 0; i < items.Count; i++)
        Console.WriteLine($"{i + 1}. {items[i].ItemName} - ${items[i].ItemPrice:F2}");

    while (true)
    {
        Console.Write("Enter item number (0 to finish): ");
        int itemNo = ReadInt();

        if (itemNo == 0) break;

        if (itemNo < 1 || itemNo > items.Count)
        {
            Console.WriteLine("Invalid item number.");
            continue;
        }

        Console.Write("Enter quantity: ");
        int qty = ReadInt();
        if (qty <= 0)
        {
            Console.WriteLine("Quantity must be at least 1.");
            continue;
        }

        FoodItem chosen = items[itemNo - 1];
        order.AddOrderedFoodItem(new OrderedFoodItem(chosen, qty));
    }

    if (order.GetOrderedFoodItems().Count == 0)
    {
        Console.WriteLine("No items selected. Order cancelled.");
        return;
    }

    string sr = ReadYesNo("Add special request? [Y/N]: ");
    if (sr == "Y")
        order.SpecialRequest = ReadNonEmpty("Enter special request: ");

    double itemsTotal = order.GetOrderedFoodItems().Sum(x => x.SubTotal);
    order.CalculateOrderTotal();
    Console.WriteLine($"Order Total: ${itemsTotal:F2} + ${Order.DeliveryFee:F2} (delivery) = ${order.OrderTotal:F2}");

    string pay = ReadYesNo("Proceed to payment? [Y/N]: ");
    if (pay == "N")
    {
        Console.WriteLine("Payment not done. Order cancelled (not saved).");
        return;
    }

    string method;
    while (true)
    {
        Console.WriteLine("Payment method:");
        Console.Write("[CC] Credit Card / [PP] PayPal / [CD] Cash on Delivery: ");
        method = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

        if (method is "CC" or "PP" or "CD") break;
        Console.WriteLine("Invalid payment method.");
    }

    order.PaymentMethod = method;
    order.IsPaid = true;
    order.OrderStatus = "Pending";


    restaurants[restId].EnqueueOrder(order);
    customersByEmail[custEmail].AddOrder(order);

    ordersById[order.OrderId] = order;


    AppendOrderToCsv(ordersFilePath, order);

    Console.WriteLine($"Order {order.OrderId} created successfully! Status: {order.OrderStatus}");
}

string PickFirstExisting(params string[] candidates)
{
    foreach (var f in candidates)
        if (File.Exists(f)) return f;

    return candidates.Length > 0 ? candidates[0] : "";
}

void BuildOrderedItemsFromString(Order order, Menu menu, string itemsStr)
{
    if (string.IsNullOrWhiteSpace(itemsStr)) return;

    var lookup = menu.GetFoodItems()
                     .GroupBy(x => x.ItemName, StringComparer.OrdinalIgnoreCase)
                     .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

    string[] parts = itemsStr.Split('|', StringSplitOptions.RemoveEmptyEntries);

    foreach (string raw in parts)
    {
        string s = raw.Trim();
        int comma = s.LastIndexOf(',');
        if (comma <= 0) continue;

        string name = s.Substring(0, comma).Trim();
        string qtyStr = s.Substring(comma + 1).Trim();

        if (!int.TryParse(qtyStr, out int qty) || qty <= 0) continue;
        if (!lookup.TryGetValue(name, out FoodItem fi)) continue; 

        order.AddOrderedFoodItem(new OrderedFoodItem(fi, qty));
    }
}

void AppendOrderToCsv(string filePath, Order order)
{
    string deliveryDate = order.DeliveryDateTime.ToString("dd/MM/yyyy");
    string deliveryTime = order.DeliveryDateTime.ToString("HH:mm");
    string created = order.OrderDateTime.ToString("dd/MM/yyyy HH:mm");
    string total = order.OrderTotal.ToString(CultureInfo.InvariantCulture);

    string itemsStr = string.Join("|",
        order.GetOrderedFoodItems().Select(x => $"{x.FoodItem.ItemName}, {x.QtyOrdered}"));

    bool needHeader = !File.Exists(filePath) || new FileInfo(filePath).Length == 0;

    using StreamWriter sw = new(filePath, append: true);

    if (needHeader)
        sw.WriteLine("OrderId,CustomerEmail,RestaurantId,DeliveryDate,DeliveryTime,DeliveryAddress,CreatedDateTime,TotalAmount,Status,Items");

    sw.WriteLine($"{order.OrderId},{EscapeCsv(order.CustomerEmail)},{EscapeCsv(order.RestaurantId)},{deliveryDate},{deliveryTime},{EscapeCsv(order.DeliveryAddress)},{created},{total},{EscapeCsv(order.OrderStatus)},{EscapeCsv(itemsStr)}");
}

string EscapeCsv(string s)
{
    s ??= "";
    if (s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r'))
        return $"\"{s.Replace("\"", "\"\"")}\"";
    return s;
}

string[] SplitCsvLine(string line)
{
    List<string> fields = new();
    bool inQuotes = false;
    StringBuilder cur = new();

    for (int i = 0; i < line.Length; i++)
    {
        char ch = line[i];

        if (ch == '"')
        {
            if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
            {
                cur.Append('"');
                i++;
            }
            else
            {
                inQuotes = !inQuotes;
            }
        }
        else if (ch == ',' && !inQuotes)
        {
            fields.Add(cur.ToString());
            cur.Clear();
        }
        else
        {
            cur.Append(ch);
        }
    }

    fields.Add(cur.ToString());
    return fields.ToArray();
}

bool IsValidEmail(string email)
{
    if (string.IsNullOrWhiteSpace(email)) return false;
    return email.Contains('@') && email.Contains('.') && !email.Contains(' ');
}

string ReadNonEmpty(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string s = (Console.ReadLine() ?? "").Trim();
        if (!string.IsNullOrWhiteSpace(s)) return s;
        Console.WriteLine("Input cannot be empty.");
    }
}

string ReadYesNo(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
        if (s == "Y" || s == "N") return s;
        Console.WriteLine("Please enter Y or N.");
    }
}

int ReadInt()
{
    while (true)
    {
        string s = (Console.ReadLine() ?? "").Trim();
        if (int.TryParse(s, out int v)) return v;
        Console.Write("Invalid number. Try again: ");
    }
}

int ReadIntRange(int min, int max)
{
    while (true)
    {
        string s = (Console.ReadLine() ?? "").Trim();
        if (int.TryParse(s, out int v) && v >= min && v <= max) return v;
        Console.Write($"Enter a number between {min} and {max}: ");
    }
}

string TrimTo(string s, int maxLen)
{
    if (string.IsNullOrEmpty(s)) return s;
    return s.Length <= maxLen ? s : s.Substring(0, maxLen - 1) + "…";
}

