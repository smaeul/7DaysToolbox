using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    public class Recipe
    {
        public Recipe(XElement recipe)
        {
            try {
                this.Name = recipe.Attribute("name").Value;
            } catch {
                throw new ArgumentException("The given XElement does not represent a valid recipe.");
            }
            this.Fill(recipe);
        }

        private void AddProperty(XElement property)
        {

        }

        private void Fill(XElement recipe)
        {

        }

        public XElement ToXElement()
        {
            XElement element = new XElement("recipe",
                new XAttribute("name", this.Name));
            return element;
        }

        public RecipeCollection Collection { get; set; }

        public String Name { get; }
    }
}
