using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    class Item
    {
        Item()
        {

        }

        public XElement AsXElement()
        {
            return new XElement("item",
                                new XAttribute("id", this.ID),
                                new XAttribute("name", this.Name));
        }

        public UInt32 ID { get; }
        public String Name { get; }
    }
}
