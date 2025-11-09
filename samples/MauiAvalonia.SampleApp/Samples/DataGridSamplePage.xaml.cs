using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace MauiAvalonia.SampleApp;

public partial class DataGridSamplePage : ContentPage
{
	readonly List<SalesOrder> sourceOrders = new()
	{
		new SalesOrder("SO-1001", "Northwind Traders", "In Progress", 1800, 0.35f),
		new SalesOrder("SO-1002", "Tailspin Toys", "Pending", 950, 0.15f),
		new SalesOrder("SO-1003", "Blue Yonder Airlines", "Completed", 4200, 1f),
		new SalesOrder("SO-1004", "Adventure Works", "In Progress", 2650, 0.55f),
		new SalesOrder("SO-1005", "Island Traders", "Pending", 1235, 0.28f),
		new SalesOrder("SO-1006", "Litware", "Completed", 5100, 1f),
		new SalesOrder("SO-1007", "Fabrikam", "In Progress", 1860, 0.65f),
		new SalesOrder("SO-1008", "Wide World Importers", "In Progress", 2395, 0.48f),
		new SalesOrder("SO-1009", "Graphic Design Institute", "Pending", 780, 0.22f)
	};

	OrderSort currentSort = OrderSort.Revenue;

	public ObservableCollection<OrderRow> Orders { get; } = new();

	bool isHighValueFilterEnabled;

	public DataGridSamplePage()
	{
		InitializeComponent();
		BindingContext = this;
		RefreshOrders();
	}

	public bool IsHighValueFilterEnabled
	{
		get => isHighValueFilterEnabled;
		set
		{
			if (isHighValueFilterEnabled == value)
				return;

			isHighValueFilterEnabled = value;
			OnPropertyChanged(nameof(IsHighValueFilterEnabled));
			RefreshOrders();
		}
	}

	void OnSortByCustomer(object? sender, EventArgs e)
	{
		currentSort = OrderSort.Customer;
		RefreshOrders();
	}

	void OnSortByStatus(object? sender, EventArgs e)
	{
		currentSort = OrderSort.Status;
		RefreshOrders();
	}

	void OnSortByRevenue(object? sender, EventArgs e)
	{
		currentSort = OrderSort.Revenue;
		RefreshOrders();
	}

	void OnHighValueFilterChanged(object? sender, CheckedChangedEventArgs e) =>
		IsHighValueFilterEnabled = e.Value;

	void RefreshOrders()
	{
		IEnumerable<SalesOrder> query = sourceOrders;

		if (IsHighValueFilterEnabled)
			query = query.Where(order => order.Total >= 2000);

		query = currentSort switch
		{
			OrderSort.Customer => query.OrderBy(order => order.Customer),
			OrderSort.Status => query.OrderBy(order => order.Status).ThenByDescending(order => order.Total),
			_ => query.OrderByDescending(order => order.Total)
		};

		Orders.Clear();

		int index = 0;
		foreach (var order in query)
		{
			var row = new OrderRow(
				order.OrderId,
				order.Customer,
				order.Status,
				order.Total,
				order.Progress,
				index % 2 == 0 ? Colors.White : Color.FromArgb("#F6F6FD"));

			Orders.Add(row);
			index++;
		}
	}

	public sealed record SalesOrder(string OrderId, string Customer, string Status, double Total, float Progress);

	public sealed record OrderRow(string OrderId, string Customer, string Status, double Total, float Progress, Color RowBackground);

	enum OrderSort
	{
		Revenue,
		Customer,
		Status
	}
}
