namespace SDTD.Config
{
    using System;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Represents a collection of items; usually, the entire contents of items.xml.
    /// </summary>
    public class ItemCollection : ObservableBaseCollection<Item>, IElementCollection
    {
        /// <summary>
        /// Creates a new, empty collection of items.
        /// </summary>
        public ItemCollection() : base()
        {
            // No additional implementation.
        }

        /// <summary>
        /// Loads all items in the XDocument into the collection.
        /// </summary>
        /// <param name="document">The XDocument to read from.</param>
        public void Load(XDocument document)
        {
            foreach (XElement element in document.Root.Elements(_elementName)) {
                UInt32 id;
                String name = element.Attribute("name")?.Value;
                if (UInt32.TryParse(element.Attribute("id")?.Value, out id) && name != null) {
                    this.Add(new Item(this, id, name));
                    foreach (XElement property in element.Elements("property")) {
                        this[name].AddProperty(property);
                    }
                }
            }
        }

        /// <summary>
        /// Saves the data for all items in the collection to a game-compatible XDocument.
        /// </summary>
        /// <param name="document">The XDocument to write to (clearing existing data in it).</param>
        public void Save(XDocument document)
        {
            document.Declaration = new XDeclaration("1.0", "utf-8", "true");
            if (document.Root != null) { document.Root.Remove(); }
            document.Add(new XElement("items"));
            document.Root.Add(this.OrderBy(i => i.ID).Select(b => b.ToXElement()));
        }
    }
}
