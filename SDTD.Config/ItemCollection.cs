using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    class ItemCollection
    {
        ItemCollection()
        {
            this.items = new Dictionary<string, Item>();
        }

        public void Add(Item item)
        {
            this.items.Add(item.Name, item);
        }

        public XElement AsXElement()
        {
            return new XElement("items",
                                this.items.Values.OrderBy(i => i.ID).Select(i => i.AsXElement()));
        }

        private Dictionary<String, Item> items;
    }
}
