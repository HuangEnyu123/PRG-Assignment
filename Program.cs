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
