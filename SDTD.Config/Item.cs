using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    public class Item
    {
        public Item(XElement item)
        {
            try {
                this.ID = Convert.ToUInt32(item.Attribute("id").Value);
                this.Name = item.Attribute("name").Value;
            } catch {
                throw new ArgumentException("The given XElement does not represent a valid item.");
            }
            this.Fill(item);
        }

        private void AddProperty(XElement property)
        {

        }

        private void Fill(XElement item)
        {

        }

        public XElement ToXElement()
        {
            XElement element = new XElement("item",
                new XAttribute("id", this.ID),
                new XAttribute("name", this.Name));
            return element;
        }

        public ItemCollection Collection { get; set; }

        public UInt32 ID { get; }
        public String Name { get; }
    }
}
