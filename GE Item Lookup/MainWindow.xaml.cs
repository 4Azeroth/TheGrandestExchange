using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GE_Item_Lookup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IdList idList = new IdList();
        public ICollectionView cvItemList;
        public List<String> titleList;
        public List<Thread> threadList = new List<Thread>();

        public MainWindow()
        {
            InitializeComponent();
            if (idList.list != null)
            {
                if (idList.list.Select(i => i.type).Distinct().ToList().Contains(null))
                {
                    Thread thread = new Thread(updateTypes);
                    thread.Start();
                    threadList.Add(thread);
                }
                cvItemList = CollectionViewSource.GetDefaultView(idList.list);
                IdListGrid.ItemsSource = cvItemList;
                cvItemList.Filter = BlankFilter;
            }
        }

        public void updateTypes()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                StatusLabel.Content = "Updating Types...";
                StatusProgress.Visibility = Visibility.Visible;
                StatusProgress.IsEnabled = true;
                StatusProgress.Maximum = idList.list.Count;
            }));
            bool internet = true;
            for (int i = 0; i < idList.list.Count && internet; i++)
            {
                if (idList.list.ElementAt(i).type == null)
                {
                    Item item = new Item(idList.list.ElementAt(i).id.ToString());
                    if (item.list != null)
                    {
                        idList.list.ElementAt(i).type = item.list.ElementAt(0).type;
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            StatusProgress.Value = i;
                            if (i < idList.list.Count)
                            {
                                StatusLabel.Content = idList.list.ElementAt(i).name + " is given type " + item.list.ElementAt(0).type;
                            }
                        }));
                    }
                    else
                    {
                        internet = false;
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            StatusProgress.Value = i;
                            if (i < idList.list.Count)
                            {
                                StatusLabel.Content = idList.list.ElementAt(i).name + " details couldn't be retrieved. Check your internet Connection!";
                            }
                        }));
                    }
                }
            }
            this.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                StatusProgress.Visibility = Visibility.Hidden;
                StatusProgress.IsEnabled = false;
                StatusLabel.Content = "Running!";
            }));
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DragRect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            cvItemList = CollectionViewSource.GetDefaultView(idList);
            MessageBox.Show(idList.ToString() + " Size of Collection: " + idList.list.Count);
        }

        private void FilterBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FilterBox.Text == "")
            {
                SolidColorBrush black = new SolidColorBrush();
                black.Color = Color.FromRgb(107, 107, 107);
                FilterBox.Text = "Type Filter Text Here";
                FilterBox.Foreground = black;
                FilterBox.FontStyle = FontStyles.Italic;

            }
        }
        public bool TextFilter(object o)
        {
            IdList.RootObject p = (o as IdList.RootObject);
            if (p == null)
                return false;

            if (p.name != null && p.name.IndexOf(FilterBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            else if (p.notes != null && p.notes.IndexOf(FilterBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            else if (p.type != null)
            {
                if (p.type.IndexOf(FilterBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            if (p.id.ToString().IndexOf(FilterBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            else
                return false;
        }

        public bool CustomTypeFilter(object o)
        {
            IdList.RootObject p = (o as IdList.RootObject);
            if (p.notes != null && p.notes.IndexOf((string)TypeBox.SelectedValue, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            else
                return false;
        }

        public bool CustomInvestmentsFilter(object o)
        {
            IdList.RootObject p = (o as IdList.RootObject);
            if (p.investments.amount > 0)
                return true;
            else
                return false;
        }

        public bool BlankFilter(object o)
        {
            IdList.RootObject p = (o as IdList.RootObject);
            if (p.name != null)
                return true;
            else
                return false;
        }

        private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cvItemList != null)
            {
                if (FilterBox.Text != "" && FilterBox.Text != "Type Filter Text Here")
                {
                    cvItemList.Filter = TextFilter;
                }
                else
                {
                    cvItemList.Filter = BlankFilter;
                }
            }
        }

        private void FilterBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FilterBox.Text == "Type Filter Text Here")
            {
                SolidColorBrush black = new SolidColorBrush();
                black.Color = Color.FromRgb(0, 0, 0);
                FilterBox.Text = String.Empty;
                FilterBox.Foreground = black;
                FilterBox.FontStyle = FontStyles.Normal;

            }
        }

        private void IdListGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IdListGrid.SelectedItem != null)
            {
                IdList.RootObject selectedItem;
                selectedItem = (IdList.RootObject)IdListGrid.SelectedItem;
                Item detailedItem = new Item(selectedItem.id.ToString());
                if (detailedItem.list != null)
                {
                    Visible_1.Visibility = Visibility.Hidden;
                    Visible_2.Visibility = Visibility.Hidden;
                    Visible_3.Visibility = Visibility.Hidden;
                    Visible_4.Visibility = Visibility.Hidden;
                    Hidden1.Visibility = Visibility.Visible;
                    Hidden2.Visibility = Visibility.Visible;
                    Hidden3.Visibility = Visibility.Visible;
                    Hidden4.Visibility = Visibility.Visible;
                    Hidden5.Visibility = Visibility.Visible;
                    Hidden6.Visibility = Visibility.Visible;
                    ClearTransactionsButton.Visibility = Visibility.Visible;
                    CostPerUnitText.Visibility = Visibility.Visible;
                    SellButton.Visibility = Visibility.Visible;
                    ProfitMarginText.Visibility = Visibility.Visible;
                    AmountInvested.Visibility = Visibility.Visible;
                    MoneyInvested.Visibility = Visibility.Visible;
                    PurchaseButton.Visibility = Visibility.Visible;
                    TransactionGrid.Visibility = Visibility.Visible;
                    TypeBox.Visibility = Visibility.Visible;
                    DescriptionBorder.Visibility = Visibility.Visible;
                    AmountToPurchaseText.Visibility = Visibility.Visible;
                    Item.RootObject itemDetails = detailedItem.list.ElementAt(0);
                    ItemImage.Source = new BitmapImage(new Uri(itemDetails.icon_large));
                    IdText.Content = "Id: " + itemDetails.id;
                    NameText.Content = "Name: " + itemDetails.name;
                    TypeText.Content = "Category: " + itemDetails.type;
                    AmountToPurchaseText.Text = "1";
                    CostPerUnitText.Text = priceToInt(itemDetails.prices.current.price).ToString();
                    MoneyInvested.Content = selectedItem.investments.money;
                    AmountInvested.Content = selectedItem.investments.amount;
                    TransactionGrid.ItemsSource = selectedItem.investments.transactions;
                    updateProfitMargin(selectedItem);
                    if (selectedItem.type == null)
                    {
                        selectedItem.type = itemDetails.type;
                    }
                    DescriptionText.Text = "Description: " + itemDetails.description;
                    PriceText.Content = "Price: " + itemDetails.prices.current.price;
                    SolidColorBrush color = new SolidColorBrush();
                    TrendLabel.Content = "Trend:";
                    if (itemDetails.prices.days30.trend == "positive")
                    {
                        color.Color = Color.FromRgb(0, 255, 0);
                        TrendText.Foreground = color;
                        TrendText.Content = itemDetails.prices.days30.change;
                    }
                    else if (itemDetails.prices.days30.trend == "negative")
                    {
                        color.Color = Color.FromRgb(255, 0, 0);
                        TrendText.Foreground = color;
                        TrendText.Content = itemDetails.prices.days30.change;
                    }
                    else
                    {
                        color.Color = Color.FromRgb(133, 133, 133);
                        TrendText.Foreground = color;
                        TrendText.Content = "Neutral";
                    }
                    this.StatusLabel.Content = "Running!";
                }
                else
                {
                    this.StatusLabel.Content = "Web Connection Error, Check your Internet Connection.";
                }
            }
        }

        private void IdListGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void TypeUpdater_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(updateTypes);
            thread.Start();
            threadList.Add(thread);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            idList.Serialize();
            foreach (Thread t in threadList)
            {
                if (t.IsAlive == true)
                {
                    t.Abort();
                }
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            Window1 window = new Window1();
            window.Show();
        }

        private void TypeBox_DropDownOpened(object sender, EventArgs e)
        {
            titleList = idList.list.Select(i => i.notes).Distinct().ToList();
            TypeBox.Items.Clear();
            TypeBox.Items.Add("");
            TypeBox.Items.Add("<Current Investments>");
            for (int i = 0; i < titleList.Count; i++)
            {
                string line = titleList.ElementAt(i);
                if (line != null && line != "")
                {
                    TypeBox.Items.Add(line);
                }
            }
        }

        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedString;
            selectedString = (string)TypeBox.SelectedValue;
            if (selectedString != null && selectedString.Equals("<Current Investments>"))
            {
                cvItemList.Filter = CustomInvestmentsFilter;
            }
            else if (selectedString != null && selectedString != "")
            {
                cvItemList.Filter = CustomTypeFilter;
            }
            else
            {
                cvItemList.Filter = BlankFilter;
            }

        }

        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            int amount;
            int costPerUnit;
            try
            {
                amount = Convert.ToInt32(AmountToPurchaseText.Text);
            }
            catch(FormatException)
            {
                amount = 0;
                ErrorLabel.Content = "Amount is not a valid number!";
            }
            try
            {
                costPerUnit = Convert.ToInt32(CostPerUnitText.Text);
                IdList.RootObject selectedItem;
                selectedItem = (IdList.RootObject)IdListGrid.SelectedItem;
                if (selectedItem == null)
                {
                    int id = Convert.ToInt32(IdText.Content.ToString().Substring(4));
                    selectedItem = idList.list.Single(i => i.id == id);
                }
                if (selectedItem != null)
                {
                    selectedItem.investments.newTransaction(amount, costPerUnit);
                    ErrorLabel.Content = "";
                    AmountInvested.Content = selectedItem.investments.amount;
                    MoneyInvested.Content = selectedItem.investments.money;
                    updateProfitMargin(selectedItem);
                }
            }
            catch (FormatException)
            {
                ErrorLabel.Content = "Cost Per Unit is not a valid number!";
            }
        }

        private void TransactionGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IInputElement element = e.MouseDevice.DirectlyOver;
            if (element != null && element is FrameworkElement)
            {
                if (((FrameworkElement)element).Parent is DataGridCell)
                {
                    IdList.RootObject selectedItem = (IdList.RootObject)IdListGrid.SelectedItem;
                    if (selectedItem == null)
                    {
                        int id = Convert.ToInt32(IdText.Content.ToString().Substring(4));
                        selectedItem = idList.list.Single(i => i.id == id);
                    }
                    IdList.Transaction selectedTransation = (IdList.Transaction)TransactionGrid.SelectedItem;
                    // Display message box
                    MessageBoxButton button = MessageBoxButton.YesNoCancel;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the transaction\nAmount: \"" + selectedTransation.amount + "\" Cost Per Unit: \"" + selectedTransation.costPerUnit + "\"?", "Remove Transaction Warning", button, icon);

                    // Process message box results 
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            selectedItem.investments.removeTransaction(selectedTransation);
                            AmountInvested.Content = selectedItem.investments.amount;
                            MoneyInvested.Content = selectedItem.investments.money;
                            updateProfitMargin(selectedItem);
                            break;
                        case MessageBoxResult.No:
                            // User pressed No button 
                            // ... 
                            break;
                        case MessageBoxResult.Cancel:
                            // User pressed Cancel button 
                            // ... 
                            break;
                    }
                }
            }
        }
        private void updateProfitMargin(IdList.RootObject item)
        {
            string priceString;
            int price;
            int result;
            string resultString;
            Item itemDetails = new Item(item.id.ToString());
            priceString = itemDetails.list.ElementAt(0).prices.current.price;
            price = priceToInt(priceString);
            result = (item.investments.amount * price) - item.investments.money;
            resultString = result.ToString();
            SolidColorBrush color = new SolidColorBrush();
            if (result>0)
            {
                color.Color = Color.FromRgb(0, 255, 0);
                resultString = "+" + resultString;
            }
            else if (result<0)
            {
                color.Color = Color.FromRgb(255, 0, 0);
            }
            else
            {
                color.Color = Color.FromRgb(255, 255, 255);
            }
            ProfitMarginText.Content = resultString;
            ProfitMarginText.Foreground = color;

        }
        private int priceToInt(string priceString)
        {
            double price;
            if (priceString.IndexOf("m") >= 0)
            {
                try
                {
                    price = Convert.ToDouble(priceString.Substring(0, priceString.Length - 1)) * 1000000;
                }
                catch (FormatException)
                {
                    MessageBox.Show("Million Improperly Parsed");
                    price = 0;
                }
            }
            else if (priceString.IndexOf("b") >= 0)
            {
                try
                {
                    price = Convert.ToDouble(priceString.Substring(0, priceString.Length - 1)) * 1000000000;
                }
                catch (FormatException)
                {
                    MessageBox.Show("Million Improperly Parsed");
                    price = 0;
                }
            }
            else if (priceString.IndexOf("k") >= 0)
            {
                try
                {
                    price = Convert.ToDouble(priceString.Substring(0, priceString.Length - 1)) * 1000;
                }
                catch (FormatException)
                {
                    MessageBox.Show("Thousand Improperly Parsed");
                    price = 0;
                }
            }
            else
            {
                try
                {
                    price = Convert.ToDouble(priceString);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Price Improperly Parsed");
                    price = 0;
                }
            }
            return Convert.ToInt32(price);
        }

        private void SellButton_Click(object sender, RoutedEventArgs e)
        {
            int amount;
            int costPerUnit;
            try
            {
                amount = Convert.ToInt32(AmountToPurchaseText.Text);
            }
            catch (FormatException)
            {
                amount = 0;
                ErrorLabel.Content = "Amount is not a valid number!";
            }
            try
            {
                costPerUnit = Convert.ToInt32(CostPerUnitText.Text);
                IdList.RootObject selectedItem;
                selectedItem = (IdList.RootObject)IdListGrid.SelectedItem;
                if (selectedItem == null)
                {
                    int id = Convert.ToInt32(IdText.Content.ToString().Substring(4));
                    selectedItem = idList.list.Single(i => i.id == id);
                }
                if (selectedItem != null)
                {
                    if (selectedItem.investments.amount - amount >= 0)
                    {
                        selectedItem.investments.sellTransaction(amount, costPerUnit);
                        ErrorLabel.Content = "";
                        AmountInvested.Content = selectedItem.investments.amount;
                        MoneyInvested.Content = selectedItem.investments.money;
                        updateProfitMargin(selectedItem);
                    }
                    else
                    {
                        MessageBox.Show("You can't sell more than you have");
                    }
                }
            }
            catch (FormatException)
            {
                ErrorLabel.Content = "Cost Per Unit is not a valid number!";
            }
        }

        private void ClearTransactionsButton_Click(object sender, RoutedEventArgs e)
        {
            IdList.RootObject selectedItem = (IdList.RootObject)IdListGrid.SelectedItem;
            if(selectedItem==null)
            {
                int id = Convert.ToInt32(IdText.Content.ToString().Substring(4));
                selectedItem = idList.list.Single(i => i.id == id);
            }
            if (selectedItem != null)
            {
                selectedItem.investments.transactions.Clear();
                selectedItem.investments.money = 0;
                selectedItem.investments.amount = 0;
                AmountInvested.Content = selectedItem.investments.amount;
                MoneyInvested.Content = selectedItem.investments.money;
                updateProfitMargin(selectedItem);
            }
        }

        private void CompanionButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.runescape.com/companion/");
        }
    }
}
