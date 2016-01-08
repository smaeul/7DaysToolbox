using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    class Recipe
    {
        Recipe()
        {

        }

        public XElement AsXElement()
        {
            return new XElement("recipe",
                                new XAttribute("name", this.Name));
        }

        public String Name { get; }
    }
}
