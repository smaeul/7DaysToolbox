using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    public class ItemCollection
    {
        private void Add(Item item)
        {
            this.items.Add(item.Name, item);
            item.Collection = this;
        }

        private void Add(XElement item)
        {
            this.Add(new Item(item));
        }

        public static ItemCollection Load(XDocument document)
        {
            ItemCollection collection = new ItemCollection();
            foreach (XElement item in document.Root.Elements("item")) {
                collection.Add(item);
            }
            return collection;
        }

        public XDocument ToXDocument()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", "true"),
                new XElement("items",
                    this.items.Values.OrderBy(i => i.ID).Select(i => i.ToXElement())));
        }

        public Int32 Count {
            get { return items.Count; }
        }

        public Item this[String name] {
            get { return this.items.ContainsKey(name) ? this.items[name] : null; }
        }

        private Dictionary<String, Item> items = new Dictionary<String, Item>();
    }
}
