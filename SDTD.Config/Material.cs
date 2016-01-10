using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    /// <summary>
    /// Represents a single material.
    /// </summary>
    public class Material
    {
        /// <summary>
        /// Initializes a material from its XML description.
        /// </summary>
        /// <param name="material">A &lt;material&gt; element in a game-compatible materials.xml format.</param>
        public Material(XElement material)
        {
            try {
                this.Name = material.Attribute("id").Value;
            } catch {
                throw new ArgumentException("The given XElement does not represent a valid material.");
            }
            this.Fill(material);
        }

        /// <summary>
        /// Adds a property to the material from its XML description.
        /// </summary>
        /// <param name="property">A &lt;property&gt; element in a game-compatible materials.xml format.</param>
        private void AddProperty(XElement property)
        {
            String name = property.Attribute("name")?.Value;
            String value = property.Attribute("value")?.Value;
            if (name == null) {
                throw new ArgumentException("The given property is missing a name.");
            }
            this.SetProperty(name, value);
        }

        /// <summary>
        /// Fills a material with properties from the given XML.
        /// </summary>
        /// <remarks>
        /// The id attribute of the given material element is ignored.
        /// </remarks>
        /// <param name="material">A &lt;material&gt; element in a game-compatible materials.xml format.</param>
        private void Fill(XElement material)
        {
            foreach (XElement property in material.Elements("property")) {
                this.AddProperty(property);
            }
        }

        /// <summary>
        /// Gets the value of the specified property as a string.
        /// </summary>
        /// <param name="name">The name of the property to retrieve.</param>
        /// <returns>The value of the property as a string, or null if the property has the default value.</returns>
        public String GetProperty(String name)
        {
            if (this.properties.ContainsKey(name)) {
                return this.properties[name];
            } else {
                return null;
            }
        }

        /// <summary>
        /// Sets a property on this material, or removes it if "value" is null.
        /// </summary>
        /// <param name="name">The name of the property to modify.</param>
        /// <param name="value">The value to set the property to, or null to remove it from this material.</param>
        public void SetProperty(String name, String value)
        {
            if (name == null) {
                throw new ArgumentNullException();
            }
            if (value == null) {
                if (this.properties.ContainsKey(name)) {
                    this.properties.Remove(name);
                }
            } else {
                if (this.properties.ContainsKey(name)) {
                    this.properties[name] = value;
                } else {
                    this.properties.Add(name, value);
                }
            }
        }

        /// <summary>
        /// Generates a game-compatible &lt;material&gt; element containing all data associated with this material.
        /// </summary>
        /// <returns>The &lt;block&gt; element representing this block.</returns>
        public XElement ToXElement()
        {
            XElement element = new XElement("material",
                new XAttribute("id", this.Name));
            foreach (KeyValuePair<String, String> propertyKVP in this.properties.OrderBy(p => p.Key)) {
                XElement propertyElement = new XElement("property",
                    new XAttribute("name", propertyKVP.Key),
                    new XAttribute("value", propertyKVP.Value));
                // These type attributes are required for the game to function
                switch (propertyKVP.Key) {
                case "Hardness":
                    propertyElement.Add(new XAttribute("type", "float"));
                    break;
                case "Mass":
                    propertyElement.Add(new XAttribute("type", "int"));
                    break;
                }
                element.Add(propertyElement);
            }
            return element;
        }

        /// <summary>
        /// The collection of materials that this material belongs to.
        /// </summary>
        public MaterialCollection Collection { get; set; }

        /// <summary>
        /// The (untranslated) internal name of the material. NOTE: This is the id attribute in the XML.
        /// </summary>
        public String Name { get; }

        /// <summary>
        /// All of the properties of the material (keyed by name), as strings.
        /// </summary>
        private Dictionary<String, String> properties = new Dictionary<String, String>();
    }
}
