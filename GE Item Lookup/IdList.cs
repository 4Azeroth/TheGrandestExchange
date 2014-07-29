using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;

namespace GE_Item_Lookup
{
    public class IdList
    {
        public IdList()
        {
            var json_data = string.Empty;
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\settings.geson"))
            {

                StreamReader sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\settings.geson");
                json_data = sr.ReadToEnd();
            }
            else
            {
                using (var w = new WebClient())
                {
                    try
                    {
                        //for (int i = 0; i < 9 && json_data == string.Empty; i++)
                        //{
                        json_data = w.DownloadString("http://us.api.rsapi.net/idlist.json");
                    }
                    catch(Exception)
                    {
                    }
                }
            }
            Deserialize(json_data);
        }

        public ObservableCollection<RootObject> list { get; set; }

        public class RootObject
        {
            public RootObject()
            {
                investments = new Investing();
            }

            public int id { get; set; }
            public string name { get; set; }
            public string notes { get; set; }
            public string type { get; set; }
            public Investing investments { get; set; }
            public override string ToString()
            {
                return "Id: " + this.id + " Name: " + this.name;
            }

        }

        public class Investing
        {
            public Investing()
            {
                transactions = new ObservableCollection<Transaction>();
            }
            public int money { get; set; }
            public int amount { get; set; }
            public ObservableCollection<Transaction> transactions { get; set; }
            public void newTransaction(int amount, int costPerUnit)
            {
                if (amount > 0 && costPerUnit >= 0)
                {
                    Transaction newTransaction = new Transaction();
                    newTransaction.amount = amount;
                    newTransaction.costPerUnit = costPerUnit;
                    this.transactions.Add(newTransaction);
                    this.money += amount * costPerUnit;
                    this.amount += amount;
                }
            }
            public void sellTransaction(int amount, int costPerUnit)
            {
                if (amount > 0 && costPerUnit >= 0)
                {
                    Transaction newTransaction = new Transaction();
                    newTransaction.amount = (-1)*amount;
                    newTransaction.costPerUnit = (-1)*costPerUnit;
                    this.transactions.Add(newTransaction);
                    this.money -= amount * costPerUnit;
                    this.amount -= amount;
                }
            }
            public void removeTransaction(Transaction transaction)
            {
                transactions.Remove(transaction);
                this.amount -= transaction.amount;
                if (transaction.costPerUnit >= 0)
                {
                    this.money -= transaction.amount * transaction.costPerUnit;
                }
                else
                {
                    this.money -= (-1)*transaction.amount * transaction.costPerUnit;
                }
            }
        }

        public class Transaction
        {
            public int amount { get; set; }
            public int costPerUnit { get; set; }
        }

        public void Deserialize(string json_data)
        {
                try
                {
                    this.list = JsonConvert.DeserializeObject<ObservableCollection<RootObject>>(json_data);
                }
                catch
                {
                    this.list = null;
                }
        }
        public void Serialize()
        {
             TextWriter tsw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\settings.geson");
             //Writing text to the file.
             tsw.Write(JsonConvert.SerializeObject(this.list));
             //Close the file.
             tsw.Close();
        }
    }
}
