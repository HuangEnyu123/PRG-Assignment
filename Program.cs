
using S10274277E_Assignment;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;

/* Student Number: S10274277E
   Student Name:   Huang Enyu (Solo)
   Partner Name:   -
*/



// restaurants: RestaurantId -> Restaurant object
// menusByRestaurant: RestaurantId -> Menu object (default menu per restaurant)
// customersByEmail: CustomerEmail -> Customer object
// ordersById: OrderId -> Order object
Dictionary<string, Restaurant> restaurants = new();
Dictionary<string, Menu> menusByRestaurant = new();
Dictionary<string, Customer> customersByEmail = new();
Dictionary<int, Order> ordersById = new();

Console.WriteLine("Welcome to the Gruberoo Food Delivery System");

// BASIC FEATURE 1 & 2: Load data from CSV files 
// PickFirstExisting lets the program work even if the "Copy" version of a file is used.

string restaurantsFile = PickFirstExisting("restaurants.csv");
string foodItemsFile = PickFirstExisting("fooditems.csv", "fooditems - Copy.csv");
string customersFile = PickFirstExisting("customers.csv");
string ordersFile = PickFirstExisting("orders.csv", "orders - Copy.csv");
string offersFile = PickFirstExisting("specialoffers.csv");

// BASIC FEATURE 1: Load restaurants (and create default menu per restaurant)
int rCount = LoadRestaurants(restaurants, menusByRestaurant, restaurantsFile);

// ADDITIONAL FEATURE: Load special offers and attach them to each Restaurant
int sCount = LoadSpecialOffers(restaurants, offersFile);
if (File.Exists(offersFile))
    Console.WriteLine($"{sCount} special offers loaded!");

// BASIC FEATURE 1: Load food items and place them under the correct restaurant menu
int fCount = LoadFoodItems(restaurants, menusByRestaurant, foodItemsFile);

// BASIC FEATURE 2: Load customers
int cCount = LoadCustomers(customersByEmail, customersFile);

// BASIC FEATURE 2: Load orders (and rebuild their ordered items list from CSV)
int oCount = LoadOrders(ordersById, restaurants, menusByRestaurant, customersByEmail, ordersFile);

// Summary of loaded data
Console.WriteLine($"{rCount} restaurants loaded!");
Console.WriteLine($"{fCount} food items loaded!");
Console.WriteLine($"{cCount} customers loaded!");
Console.WriteLine($"{oCount} orders loaded!");

// === Main Menu Loop ===
// User can repeatedly select features until Exit is chosen.
while (true)
{
    Console.WriteLine("\n===== Gruberoo Food Delivery System =====");
    Console.WriteLine("1. List all restaurants and menu items");
    Console.WriteLine("2. List all orders");
    Console.WriteLine("3. Create a new order");
    Console.WriteLine("4. Bulk process pending orders (today)");
    Console.WriteLine("0. Exit");
    Console.Write("Enter your choice: ");

    // Input validation: only accept 0..4
    int choice = ReadIntRange(0, 4);

    if (choice == 0)
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    switch (choice)
    {
        // BASIC FEATURE 3
        case 1:
            ListRestaurantsAndMenuItems(restaurants);
            break;

        // BASIC FEATURE 4
        case 2:
            ListAllOrders(ordersById, customersByEmail, restaurants);
            break;
        // BASIC FEATURE 5 (+ additional feature inside: special offer)
        case 3:
            CreateNewOrder(ordersById, restaurants, menusByRestaurant, customersByEmail, ordersFile);
            break;
        // ADVANCED FEATURE (a)
        case 4:
            BulkProcessPendingOrdersForToday(restaurants,ordersById, ordersFile);
            break;
    }
}
// BASIC FEAUTURE 1: Load restaurant.csv
// Creates Restaurant objects
// Creates one default Menu per restaurant and stores in menusByRestaurant 
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
        _ = sr.ReadLine();  // skip header row 

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] p = SplitCsvLine(line);
            if (p.Length < 3) continue;

            string restId = p[0].Trim();
            string restName = p[1].Trim();
            string restEmail = p[2].Trim();

            // Basic validation to prevent creating invalid objects
            if (string.IsNullOrWhiteSpace(restId) ||
                string.IsNullOrWhiteSpace(restName) ||
                string.IsNullOrWhiteSpace(restEmail))
                continue;

            // Avoid duplicates
            if (restaurants.ContainsKey(restId)) continue;

            Restaurant r = new Restaurant(restId, restName, restEmail);
            restaurants[restId] = r;

            // Create a default menu for reastaurant (so food items can be added)
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

//BASIC FEATURE 1: Load fooditems.csv
//Create FoodItem objects
//Add them into the correct restaurant's Menu
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
        _ = sr.ReadLine();  // skip header row

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] p = SplitCsvLine(line);
            if (p.Length < 4) continue;

            string restId = p[0].Trim();
            string itemName = p[1].Trim();
            string itemDesc = p[2].Trim();
            string priceStr = p[3].Trim();

            // Ensure restaurant/menu exists before adding items
            if (!restaurants.ContainsKey(restId) || !menusByRestaurant.ContainsKey(restId))
                continue;

            if (string.IsNullOrWhiteSpace(itemName) || string.IsNullOrWhiteSpace(itemDesc))
                continue;
            // Price validation
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


// Basic Feature 2: Load customers.csv
// Creates Customer objects stored in customersByEmail
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
        _ = sr.ReadLine(); // skip header row

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] p = SplitCsvLine(line);
            if (p.Length < 2) continue;

            string name = p[0].Trim();
            string email = p[1].Trim();
            // Basic validation for customer data
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email)) continue;
            if (!IsValidEmail(email)) continue;
            // Avoid duplicates
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

//BASIC FEATURE 2: Load orders.csv
// Creates Order Objects
// Rebuilds ordered items list from the Items field in CSV
// Enqueues orders into Restaurant queue and adds to Customer order list 
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
        _ = sr.ReadLine(); // skip header row

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine() ?? "";
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
            // Validate that referenced customer and restaurant exist
            if (ordersById.ContainsKey(orderId)) continue;
            if (!customersByEmail.ContainsKey(custEmail)) continue;
            if (!restaurants.ContainsKey(restId) || !menusByRestaurant.ContainsKey(restId)) continue;

            if (string.IsNullOrWhiteSpace(address)) continue;
            // Parse delivery date and time
            if (!DateTime.TryParseExact(deliveryDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dDate))
                continue;

            if (!DateTime.TryParseExact(deliveryTimeStr, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dTime))
                continue;

            DateTime deliveryDt = dDate.Date.Add(dTime.TimeOfDay);
            // Parse created datetime; if invalid, default to now
            if (!DateTime.TryParseExact(createdStr, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime createdDt))
                createdDt = DateTime.Now;
            // Create order object
            Order o = new Order(orderId, custEmail, restId, createdDt, deliveryDt, address);
            // Load status (default to Pending if blank)
            o.OrderStatus = string.IsNullOrWhiteSpace(status) ? "Pending" : status;
            // Rebuild ordered items from the CSV Items string
            BuildOrderedItemsFromString(o, menusByRestaurant[restId], itemsStr);
            // If Items is empty, fall back to using TotalAmount from CSV
            if (o.GetOrderedFoodItems().Count == 0 &&
                double.TryParse(totalStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double fileTotal) &&
                fileTotal >= 0)

            {
                o.SetTotalFromFile(fileTotal);
            }
            
            // Add order into restaurant queue and customer order list
            restaurants[restId].EnqueueOrder(o);
            customersByEmail[custEmail].AddOrder(o);
            // Track in dictionary for listing and saving
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

//ADDITIONAL FEATURE: Load specialoffers.csv
//Attaches SpecialOffer objects to each Restaurant
//These offers are shown during CreateNewOrder and can affect OrderTotal
int LoadSpecialOffers(Dictionary<string, Restaurant> restaurants, string filePath)
{
    if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        return 0;

    // Build a lookup by RestaurantName to match your specialoffers.csv format
    var byName = restaurants.Values.ToDictionary(
        r => r.RestaurantName.Trim(),
        r => r,
        StringComparer.OrdinalIgnoreCase
    );

    int loaded = 0;

    try
    {
        using StreamReader sr = new(filePath);
        _ = sr.ReadLine();// skip header row


        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] p = SplitCsvLine(line);
            if (p.Length < 4) continue;

            string restName = p[0].Trim();  
            string code = p[1].Trim();
            string desc = p[2].Trim();
            string discStr = p[3].Trim();
            // Only attach offer if the restaurant exists
            if (!byName.TryGetValue(restName, out var rest))
                continue;
            // Discount parsing:
            // - "-" means no % discount (used for offers like Free Delivery)
            // - otherwise parse the number (e.g., 10 means 10% off)
            double discount = 0;
            if (discStr != "-" &&
                !double.TryParse(discStr, NumberStyles.Any, CultureInfo.InvariantCulture, out discount))
                discount = 0;

            rest.AddSpecialOffer(new SpecialOffer(code, desc, discount));
            loaded++;
        }
    }
    catch { }

    return loaded;
}
//BASIC FEATURE 3: List all restaurants and their menu items
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

//BASIC FEATURE 4: List all orders in a formatted table
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

    // Column widths for alignment in console output
    const int W_ID = 10;
    const int W_CUST = 12;
    const int W_REST = 14;
    const int W_DT = 20;      
    const int W_AMT = 8;      
    const int W_STATUS = 10;

    // Header row
    Console.WriteLine(
        $"{"Order ID",-W_ID} " +
        $"{"Customer",-W_CUST} " +
        $"{"Restaurant",-W_REST} " +
        $"{"Delivery Date/Time",-W_DT} " +
        $"{"Amount",-W_AMT} " +
        $"{"Status",-W_STATUS}"
    );

    // Divider row (same width as header)
    Console.WriteLine(
        $"{new string('-', W_ID),-W_ID} " +
        $"{new string('-', W_CUST),-W_CUST} " +
        $"{new string('-', W_REST),-W_REST} " +
        $"{new string('-', W_DT),-W_DT} " +
        $"{new string('-', W_AMT),-W_AMT} " +
        $"{new string('-', W_STATUS),-W_STATUS}"
    );

    foreach (Order o in ordersById.Values.OrderBy(x => x.OrderId))
    {
        // Map customer email to customer name for display
        string custName = customersByEmail.TryGetValue(o.CustomerEmail, out var c)
            ? c.CustomerName
            : o.CustomerEmail;
        // Map restaurant ID to restaurant name for display
        string restName = restaurants.TryGetValue(o.RestaurantId, out var r)
            ? r.RestaurantName
            : o.RestaurantId;

        string deliveryDT = o.DeliveryDateTime.ToString("dd/MM/yyyy HH:mm");

        // Format amount with currency symbol
        string amount = "$" + o.OrderTotal.ToString("0.00");

        // Trim long strings to keep alignment
        custName = TrimTo(custName, W_CUST);
        restName = TrimTo(restName, W_REST);

        Console.WriteLine(
            $"{o.OrderId,-W_ID} " +
            $"{custName,-W_CUST} " +
            $"{restName,-W_REST} " +
            $"{deliveryDT,-W_DT} " +
            $"{amount,-W_AMT} " +
            $"{o.OrderStatus,-W_STATUS}"
        );
    }
}

//BASIC FEATURE 5: Create a new order
// Includes:
// - Selecting items + quantity
// - One special request (applied to items)
// - ADDITIONAL FEATURE: apply special offer (discount or free delivery)
// - Payment confirmation + method
// - Save order to orders.csv
void CreateNewOrder(Dictionary<int, Order> ordersById,
                    Dictionary<string, Restaurant> restaurants,
                    Dictionary<string, Menu> menusByRestaurant,
                    Dictionary<string, Customer> customersByEmail,
                    string ordersFilePath)
{
    Console.WriteLine("\nCreate New Order");
    Console.WriteLine("================");
    // Step 1: Validate customer email
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
    //  Step 2: Validate restaurant ID 
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
    //  Step 3: Delivery date input 
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
    //  Step 4: Delivery time input 
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
    // Combine date + time into a single DateTime
    DateTime deliveryDt = deliveryDate.Date.Add(deliveryTime.TimeOfDay);
    // Business rule: delivery cannot be in the past
    if (deliveryDt < DateTime.Now)
    {
        Console.WriteLine("Delivery date/time cannot be in the past. Order cancelled.");
        return;
    }
    //  Step 5: Delivery address input 
    string address = ReadNonEmpty("Enter Delivery Address: ");
    Console.WriteLine();


    // Generate new Order ID (sequential: max + 1; default start at 1001)
    int newOrderId = (ordersById.Count == 0) ? 1001 : ordersById.Keys.Max() + 1;

    // Create order object
    Order order = new Order(newOrderId, custEmail, restId, DateTime.Now, deliveryDt, address);

    // Retrieve menu items for selected restaurant
    Menu menu = menusByRestaurant[restId];
    List<FoodItem> items = menu.GetFoodItems().ToList();

    if (items.Count == 0)
    {
        Console.WriteLine("This restaurant has no food items. Order cancelled.");
        return;
    }

    // Display available food items (user selects by number)
    Console.WriteLine("Available Food Items:");
    for (int i = 0; i < items.Count; i++)
        Console.WriteLine($"{i + 1}. {items[i].ItemName} - ${items[i].ItemPrice:F2}");
    //  Step 6: Select items and quantities (repeat until user enters 0) 
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
        // Copy food item so customisation for this order does not affect the menu item
        FoodItem customisedCopy = chosen.CopyForOrder("");

        order.AddOrderedFoodItem(new OrderedFoodItem(customisedCopy, qty));
    }

    // Must select at least one item
    if (order.GetOrderedFoodItems().Count == 0)
    {
        Console.WriteLine("No items selected. Order cancelled.");
        return;
    }

    // --- Step 7: One special request (applied to all ordered items)
    string customiseRequest = "";
    string sr = ReadYesNo("Add special request? [Y/N]: ");
    if (sr == "Y")
    {
        customiseRequest = ReadNonEmpty("Enter special request: ");
    }
   
    foreach (var oi in order.GetOrderedFoodItems())
    {
        oi.FoodItem.SetCustomise(customiseRequest);
    }
    //ADDITIONAL FEATURE: Apply Special Offer (discount or free delivery) 
    Restaurant chosenRestaurant = restaurants[restId];
    var offers = chosenRestaurant.GetSpecialOffers();

    if (offers.Count > 0)
    {
        string useOffer = ReadYesNo("Apply special offer? [Y/N]: ");
        if (useOffer == "Y")
        {
            Console.WriteLine("Available Special Offers:");
            foreach (var off in offers)
            {
                // Display discount text (or show that there is no % discount)
                string discText = off.Discount > 0
                    ? $"{off.Discount:0.##}% off"
                    : "No % discount";

                Console.WriteLine($"- {off.OfferCode}: {off.OfferDesc} ({discText})");
            }

            // Validate entered offer code
            while (true)
            {
                string code = ReadNonEmpty("Enter Offer Code: ").ToUpperInvariant();

                var selected = offers.FirstOrDefault(x =>
                    x.OfferCode.Equals(code, StringComparison.OrdinalIgnoreCase));

                if (selected == null)
                {
                    Console.WriteLine("Invalid offer code.");
                    continue;
                }
                // Apply offer to order (recalculates total)
                order.ApplySpecialOffer(selected);
                Console.WriteLine($"Offer applied: {selected.OfferCode}");
                break;
            }
        }
    }
    Console.WriteLine();

    // Ensure latest total is calculated (after items + offer)
    order.CalculateOrderTotal();

    //Display total breakdown 
    double rawItemsTotal = order.GetOrderedFoodItems().Sum(x => x.SubTotal);
    double deliveryFee = order.FreeDeliveryApplied ? 0.0 : Order.DeliveryFee;
    double discountedItemsTotal = order.OrderTotal - deliveryFee;
    double discountAmount = rawItemsTotal * (order.DiscountPercentApplied / 100.0);

    if (order.DiscountPercentApplied > 0)
    {
        Console.WriteLine(
            $"Order Total: ${rawItemsTotal:F2} - ${discountAmount:F2} ({order.DiscountPercentApplied:0.##}%) = " +
            $"${discountedItemsTotal:F2} + ${deliveryFee:F2} (delivery) = ${order.OrderTotal:F2}"
        );
    }
    else
    {
        Console.WriteLine(
            $"Order Total: ${rawItemsTotal:F2} + ${deliveryFee:F2} (delivery) = ${order.OrderTotal:F2}"
        );
    }

    //Step 8: Proceed to payment? 
    string pay = ReadYesNo("Proceed to payment? [Y/N]: ");
    if (pay == "N")
    {
        Console.WriteLine("Payment not done. Order cancelled (not saved).");
        return;
    }
    Console.WriteLine();

    //Step 9: Choose payment method 
    string method;
    while (true)
    {
        Console.WriteLine("Payment method:");
        Console.Write("[CC] Credit Card / [PP] PayPal / [CD] Cash on Delivery: ");
        method = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

        if (method is "CC" or "PP" or "CD") break;
        Console.WriteLine("Invalid payment method.");
    }
    Console.WriteLine();

    // Update order payment/status fields
    order.PaymentMethod = method;
    order.IsPaid = true;
    order.OrderStatus = "Pending";

    // Add into restaurant queue and customer order list
    restaurants[restId].EnqueueOrder(order);
    customersByEmail[custEmail].AddOrder(order);

    // Track in orders dictionary
    ordersById[order.OrderId] = order;

    // Persist into orders CSV (append)
    AppendOrderToCsv(ordersFilePath, order);

    Console.WriteLine($"Order {order.OrderId} created successfully! Status: {order.OrderStatus}");
}


// ADVANCED FEATURE (a) – Design interpretation:
// Only Pending orders with delivery date = TODAY are processed.
// Pending orders for tomorrow or future dates are intentionally ignored.
// Alternative interpretation would process all Pending orders,
// but this implementation applies the 1-hour rule only to same-day deliveries.


void BulkProcessPendingOrdersForToday(
    Dictionary<string, Restaurant> restaurants,
    Dictionary<int, Order> ordersById,
    string ordersFilePath)
{
    Console.WriteLine("\nBulk Processing Pending Orders (Today)");
    Console.WriteLine("=====================================");

    if (restaurants == null || restaurants.Count == 0)
    {
        Console.WriteLine("No restaurants loaded.");
        return;
    }

    if (ordersById == null || ordersById.Count == 0)
    {
        Console.WriteLine("No orders loaded.");
        return;
    }

    DateTime now = DateTime.Now;
    DateTime today = now.Date;

    List<Order> pendingToday = new();

    // Scan through each restaurant's order queue to find Pending orders for today
    foreach (Restaurant r in restaurants.Values)
    {
        Queue<Order> q = r.GetOrderQueue();
        if (q == null || q.Count == 0) continue;

        foreach (Order o in q)
        {
            if (o == null) continue;

            if (o.OrderStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase) &&
                o.DeliveryDateTime.Date == today)
            {
                pendingToday.Add(o);
            }
        }
    }

    Console.WriteLine($"Total number of PENDING orders for today ({today:dd/MM/yyyy}): {pendingToday.Count}");

    int processed = 0, preparing = 0, rejected = 0;

    // Process each pending order based on time to delivery
    foreach (Order o in pendingToday.OrderBy(x => x.DeliveryDateTime))
    {
        TimeSpan timeToDelivery = o.DeliveryDateTime - now;

        if (timeToDelivery < TimeSpan.FromHours(1))
        {
            // Less than 1 hour before delivery => auto reject
            o.OrderStatus = "Rejected";
            rejected++;
        }
        else
        {
            // Otherwise => auto prepare
            o.OrderStatus = "Preparing";
            preparing++;
        }

        processed++;
    }
    // Summary stats required by advanced feature
    Console.WriteLine("\nSummary Statistics");
    Console.WriteLine("------------------");
    Console.WriteLine($"Orders processed: {processed}");
    Console.WriteLine($"Preparing orders: {preparing}");
    Console.WriteLine($"Rejected orders : {rejected}");

    // Percentage processed against all orders in the system
    double percentage = ordersById.Count == 0 ? 0 : (processed * 100.0 / ordersById.Count);
    Console.WriteLine($"Auto-processed percentage (processed / all orders): {percentage:F2}%");

    // Save updated statuses back into orders CSV (overwrite)
    SaveAllOrdersToCsv(ordersFilePath, ordersById);
    Console.WriteLine("\nUpdated order statuses saved to CSV.");
}

// Helper: Rewrite the entire orders CSV from ordersById
// Used to persist bulk-processing status updates
void SaveAllOrdersToCsv(string filePath, Dictionary<int, Order> ordersById)
{
    if (string.IsNullOrWhiteSpace(filePath))
    {
        Console.WriteLine("WARNING: orders file path is empty. Status changes NOT saved.");
        return;
    }

    using StreamWriter sw = new(filePath, append: false);

    sw.WriteLine("OrderId,CustomerEmail,RestaurantId,DeliveryDate,DeliveryTime,DeliveryAddress,CreatedDateTime,TotalAmount,Status,Items");

    foreach (Order o in ordersById.Values.Where(x => x != null).OrderBy(x => x.OrderId))
    {
        string deliveryDate = o.DeliveryDateTime.ToString("dd/MM/yyyy");
        string deliveryTime = o.DeliveryDateTime.ToString("HH:mm");
        string created = o.OrderDateTime.ToString("dd/MM/yyyy HH:mm");
        string total = o.OrderTotal.ToString(CultureInfo.InvariantCulture);

        // Items field is stored as: "ItemName, Qty|ItemName, Qty|..."
        string itemsStr = string.Join("|",
            o.GetOrderedFoodItems().Select(x => $"{x.FoodItem.ItemName}, {x.QtyOrdered}"));

        sw.WriteLine($"{o.OrderId},{EscapeCsv(o.CustomerEmail)},{EscapeCsv(o.RestaurantId)},{deliveryDate},{deliveryTime},{EscapeCsv(o.DeliveryAddress)},{created},{total},{EscapeCsv(o.OrderStatus)},{EscapeCsv(itemsStr)}");
    }
}



// Helper: Choose the first file that exists from candidates
string PickFirstExisting(params string[] candidates)
{
    foreach (var f in candidates)
        if (File.Exists(f)) return f;

    return candidates.Length > 0 ? candidates[0] : "";
}



// Helper: Convert Items string from CSV into OrderedFoodItems in the Order
// Format expected: "ItemName, Qty|ItemName, Qty|..."
void BuildOrderedItemsFromString(Order order, Menu menu, string itemsStr)
{
    if (string.IsNullOrWhiteSpace(itemsStr)) return;

    // Lookup menu items by name (case-insensitive) so we can rebuild ordered items
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
        if (!lookup.TryGetValue(name, out var fi)) continue; 

        order.AddOrderedFoodItem(new OrderedFoodItem(fi, qty));
    }
}

// Helper: Append a newly created order into the orders CSV
// (Used in Basic Feature 5 when order is created successfully)
void AppendOrderToCsv(string filePath, Order order)
{
    string deliveryDate = order.DeliveryDateTime.ToString("dd/MM/yyyy");
    string deliveryTime = order.DeliveryDateTime.ToString("HH:mm");
    string created = order.OrderDateTime.ToString("dd/MM/yyyy HH:mm");
    string total = order.OrderTotal.ToString(CultureInfo.InvariantCulture);

    string itemsStr = string.Join("|",
        order.GetOrderedFoodItems().Select(x => $"{x.FoodItem.ItemName}, {x.QtyOrdered}"));

    // If file doesn't exist or empty, write header first
    bool needHeader = !File.Exists(filePath) || new FileInfo(filePath).Length == 0;

    using StreamWriter sw = new(filePath, append: true);

    if (needHeader)
        sw.WriteLine("OrderId,CustomerEmail,RestaurantId,DeliveryDate,DeliveryTime,DeliveryAddress,CreatedDateTime,TotalAmount,Status,Items");

    sw.WriteLine($"{order.OrderId},{EscapeCsv(order.CustomerEmail)},{EscapeCsv(order.RestaurantId)},{deliveryDate},{deliveryTime},{EscapeCsv(order.DeliveryAddress)},{created},{total},{EscapeCsv(order.OrderStatus)},{EscapeCsv(itemsStr)}");
}


// Helper: Escape CSV fields that contain commas/quotes/newlines
string EscapeCsv(string s)
{
    s ??= "";
    if (s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r'))
        return $"\"{s.Replace("\"", "\"\"")}\"";
    return s;
}

// Helper: CSV line splitter that supports quoted commas
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

// Helper: Basic email validation used for input checking
bool IsValidEmail(string email)
{
    if (string.IsNullOrWhiteSpace(email)) return false;
    return email.Contains('@') && email.Contains('.') && !email.Contains(' ');
}

// Helper: Read non-empty input from console
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

// Helper: Read Y/N input only
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

// Helper: Read an integer (re-prompts until valid)
int ReadInt()
{
    while (true)
    {
        string s = (Console.ReadLine() ?? "").Trim();
        if (int.TryParse(s, out int v)) return v;
        Console.Write("Invalid number. Try again: ");
    }
}

// Helper: Read an integer within a range [min..max]
// Used for menu selection validation
int ReadIntRange(int min, int max)
{
    while (true)
    {
        string s = (Console.ReadLine() ?? "").Trim();
        if (int.TryParse(s, out int v) && v >= min && v <= max) return v;
        Console.Write($"Enter a number between {min} and {max}: ");
    }
}
// Helper: Trim long strings for table formatting (adds ellipsis)
string TrimTo(string s, int maxLen)
{
    if (string.IsNullOrEmpty(s)) return s;
    return s.Length <= maxLen ? s : s.Substring(0, maxLen - 1) + "…";
}

