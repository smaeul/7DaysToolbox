using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    class RecipeCollection
    {
        RecipeCollection()
        {
            this.recipes = new Dictionary<string, Recipe>();
        }

        public void Add(Recipe recipe)
        {
            this.recipes.Add(recipe.Name, recipe);
        }

        public XElement AsXElement()
        {
            return new XElement("recipes",
                                this.recipes.Values.OrderBy(r => r.Name).Select(r => r.AsXElement()));
        }

        private Dictionary<String, Recipe> recipes;
    }
}
