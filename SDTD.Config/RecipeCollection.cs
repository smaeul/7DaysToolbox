using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    public class RecipeCollection
    {
        private void Add(Recipe recipe)
        {
            if (!recipes.ContainsKey(recipe.Name)) {
                this.recipes.Add(recipe.Name, new List<Recipe>());
            }
            this.recipes[recipe.Name].Add(recipe);
            recipe.Collection = this;
        }

        private void Add(XElement recipe)
        {
            this.Add(new Recipe(recipe));
        }

        public static RecipeCollection Load(XDocument document)
        {
            RecipeCollection collection = new RecipeCollection();
            foreach (XElement recipe in document.Root.Elements("recipe")) {
                collection.Add(recipe);
            }
            return collection;
        }

        public XDocument ToXDocument()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", "true"),
                new XElement("recipes",
                    this.recipes.Values.OrderBy(rl => rl.First().Name).Select(rl => rl.Select(r => r.ToXElement()))));
        }

        public Int32 Count {
            get { return recipes.Values.Select(rl => rl.Count).Sum(); }
        }

        public List<Recipe> this[String name] {
            get { return this.recipes.ContainsKey(name) ? this.recipes[name] : null; }
        }

        private Dictionary<String, List<Recipe>> recipes = new Dictionary<String, List<Recipe>>();
    }
}
