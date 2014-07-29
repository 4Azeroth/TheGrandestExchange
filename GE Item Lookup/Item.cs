using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;

namespace GE_Item_Lookup
{
    public class Item
    {
        public Item(string ids)
        {
            this.Deserialize(ids);
        }

        void list_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RootObject newestItem = list.Last();
            if (newestItem.name.Equals(null))
            {
                list.Remove(newestItem);
            }
            throw new NotImplementedException();
        }

        public ObservableCollection<RootObject> list { get; set; }

        public class Current
        {
            public string trend { get; set; }
            public string price { get; set; }
        }

        public class Today
        {
            public string trend { get; set; }
            public string price { get; set; }
        }

        public class Days30
        {
            public string trend { get; set; }
            public string change { get; set; }
        }

        public class Days90
        {
            public string trend { get; set; }
            public string change { get; set; }
        }

        public class Days180
        {
            public string trend { get; set; }
            public string change { get; set; }
        }

        public class Prices
        {
            public Current current { get; set; }
            public Today today { get; set; }
            public Days30 days30 { get; set; }
            public Days90 days90 { get; set; }
            public Days180 days180 { get; set; }
            public string exact { get; set; }
        }

        public class RootObject
        {
            public int id { get; set; }
            public string icon { get; set; }
            public string icon_large { get; set; }
            public string type { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string membersitem { get; set; }
            public Prices prices { get; set; }
        }

        public void Deserialize(string ids)
        {
            using (var w = new WebClient())
            {
                var json_data = string.Empty;
                json_data = w.DownloadString("http://us.api.rsapi.net/ge/item/" + ids + ".json");
                //for (int i = 0; i < 9 && json_data == string.Empty; i++)
                /*{
                    if (json_data != null)
                    {
                        TextWriter tsw = new StreamWriter(@"C:\\Users\\zteisber\\Documents\\itemlist.txt", true);

                        //Writing text to the file.
                        tsw.WriteLine(json_data + System.Environment.NewLine);


                        //Close the file.
                        tsw.Close();
                    }
                    }*/
                try
                {
                    this.list = JsonConvert.DeserializeObject<ObservableCollection<RootObject>>(json_data);
                }
                catch
                {
                    this.list = null;
                }
            }
        }
    }
}