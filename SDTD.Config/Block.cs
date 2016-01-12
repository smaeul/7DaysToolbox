namespace SDTD.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Represents a single block.
    /// </summary>
    public class Block : ObservableBase<Block>, IElement
    {
        private Dictionary<String, List<Drop>> _events = new Dictionary<String, List<Drop>>();
        private Dictionary<String, String> _properties = new Dictionary<String, String>();

        /// <summary>
        /// Create a new <c>Block</c> instance with the specified attributes, belonging to the given collection.
        /// </summary>
        /// <param name="collection">The collection to which the block belongs.</param>
        /// <param name="id">The numeric identifier of the block.</param>
        /// <param name="name">The (untranslated) internal name of the block.</param>
        public Block(BlockCollection collection, UInt32 id, String name) : base(collection, id, name)
        {
            // No additional implementation.
        }

        /// <summary>
        /// The block that this block inherits from.
        /// </summary>
        public Block Extends {
            get {
                String block = this.GetProperty("Extends");
                return (block != null && this.Collection.ContainsKey(block)) ? this.Collection[block] : null;
            }
        }

        /// <summary>
        /// Adds a drop to the block from its XML description.
        /// </summary>
        /// <param name="drop">A &lt;drop&gt; element in a game-compatible blocks.xml format.</param>
        public void AddDrop(XElement drop)
        {
            String eventName = drop.Attribute("event")?.Value;
            if (eventName == null) { throw new ArgumentException("The given drop is missing an event name."); }
            if (!this._events.ContainsKey(eventName)) {
                this._events.Add(eventName, new List<Drop>());
            }
            this._events[eventName].Add(new Drop(this, drop));
        }

        /// <summary>
        /// Adds a property to the block from its XML description.
        /// </summary>
        /// <remarks>
        /// This function works on either a name/value property element, or a class property element.
        /// In the case of a class, it calls the other overload for each property in the class.
        /// </remarks>
        /// <param name="property">A &lt;property&gt; element in a game-compatible blocks.xml format.</param>
        public void AddProperty(XElement property)
        {
            String className = property.Attribute("class")?.Value;
            if (className != null) {
                foreach (XElement subprop in property.Elements("property")) {
                    this.AddProperty(subprop, className);
                }
            } else {
                this.AddProperty(property, null);
            }
        }

        /// <summary>
        /// Gets the value of the specified property as a string, from this block or the block it extends.
        /// </summary>
        /// <param name="name">The name of the property to retrieve.</param>
        /// <returns>The value of the property as a string, or null if the property has the default value.</returns>
        public String GetProperty(String name)
        {
            if (this._properties.ContainsKey(name)) {
                return this._properties[name];
            } else if (name == "Extends") {
                // Explicitly do not recurse when getting the parent block's name.
                return null;
            } else {
                return this.Extends?.GetProperty(name);
            }
        }

        /// <summary>
        /// Sets a property on the block, removing it if it matches the extended block, or if the value is null.
        /// </summary>
        /// <param name="name">The name of the property to modify.</param>
        /// <param name="value">The value to set the property to, or null to inherit/remove from this block.</param>
        /// <returns>True if the property was changed, or false if no changes were made.</returns>
        public Boolean SetProperty(String name, String value)
        {
            if (name == null) { throw new ArgumentNullException(nameof(name)); }

            List<KeyValuePair<String, String>> oldProperties = null;
            String oldValue = this.GetProperty(name);

            if (name == "Extends") {
                oldProperties = new List<KeyValuePair<string, string>>(_properties);
            }
            if (this._properties.ContainsKey(name)) {
                if (this.Extends?.GetProperty(name) == value || value == null) {
                    this._properties.Remove(name);
                } else {
                    this._properties[name] = value;
                }
            } else {
                if (this.Extends?.GetProperty(name) != value && value != null) {
                    this._properties.Add(name, value);
                } else {
                    return false;
                }
            }
            if (this.GetProperty(name) == oldValue) {
                return false;
            } else {
                this.OnPropertyChanged(name);
            }
            if (name == "Extends") {
                // Restore all of the other property values.
                foreach (KeyValuePair<String, String> property in oldProperties.Where(k => k.Key != "Extends")) {
                    this.SetProperty(property.Key, property.Value);
                }
            }
            return true;
        }

        /// <summary>
        /// Generates a game-compatible XML description of the block.
        /// </summary>
        /// <returns>The &lt;block&gt; element representing this block.</returns>
        public XElement ToXElement()
        {
            XElement element = new XElement("block",
                new XAttribute("id", this.ID),
                new XAttribute("name", this.Name));
            foreach (KeyValuePair<String, String> propertyKVP in this._properties.OrderBy(p => p.Key)) {
                XElement propertyElement = new XElement("property",
                    new XAttribute("name", propertyKVP.Key),
                    new XAttribute("value", propertyKVP.Value));
                switch (propertyKVP.Key) {
                case "PlantGrowing.GrowOnTop":
                    if (this.Name == "growableCornTop2") {
                        propertyElement.Add(new XAttribute("param1", "3"),
                            new XAttribute("param2", "grownUpCornTop1"));
                    }
                    break;
                case "RepairBlock.Item":
                    propertyElement = new XElement("property",
                        new XAttribute("class", "RepairItems"),
                        new XElement("property",
                            new XAttribute("name", propertyKVP.Value),
                            new XAttribute("value", this._properties["RepairBlock.ItemCount"])));
                    break;
                case "RepairBlock.ItemCount":
                    continue;
                }
                element.Add(propertyElement);
            }
            foreach (KeyValuePair<String, List<Drop>> eventKVP in this._events.OrderBy(e => e.Key)) {
                foreach (Drop drop in eventKVP.Value) {
                    XElement dropElement = new XElement("drop",
                        new XAttribute("event", eventKVP.Key),
                        new XAttribute("count", drop.Count));
                    if (dropElement.Attribute("count").Value != "0") {
                        if (drop.Name != null && drop.Name != this.Name) {
                            dropElement.Add(new XAttribute("name", drop.Name));
                        }
                        if (drop.Probability != 1F) {
                            dropElement.Add(new XAttribute("prob", drop.Probability));
                        }
                        if (drop.StickChance != 0F) {
                            dropElement.Add(new XAttribute("stick_chance", drop.StickChance));
                        }
                        if (drop.ToolCategory != null) {
                            dropElement.Add(new XAttribute("tool_category", drop.ToolCategory));
                        }
                    }
                    element.Add(dropElement);
                }
            }
            return element;
        }

        /// <summary>
        /// Adds a property to the block from its XML description.
        /// </summary>
        /// <remarks><para>
        /// Except for RepairItems (see below), this function flattens all classed properties to the class.name format
        /// This makes them easier to work with (no need for custom classes), and it makes the generated XML file
        /// smaller. With two exceptions, any classed property can be represented in either way (nested elements or
        /// class.name). Map.Color does not work when nested, and RepairItems.* does not work when flattened. It
        /// appears that at one point, the Map class had multiple properties, and therefore was nested. See the
        /// commented out part of block #25 forestGround as of A13.6.
        /// </para><para>
        /// The way RepairItems is implemented is just dumb. Putting the item name in the name of the property creates
        /// an unbounded number of possible properties. It also requires specific code to parse it, which is why it
        /// cannot be flattened. I've copied the names from the UpgradeBlock.* properties for internal representation.
        /// The only way their layout makes sense is if there are multiple repair paths, which there is currently no
        /// UI or control for. If that was to be implemented, it would go along with multiple upgrade, so both would
        /// be changed at the same time anyway.
        /// </para><para>
        /// This function ignores param1/2 except where special-cased. This has not caused any obvious problems.
        /// * For CanPickup, param1 is the item that is picked up. This is completely replaced by PickupTarget.
        /// * For Model, param1 is (almost) always the same value, and removing it doesn't seem to change anything.
        /// * For PlantGrowing.GrowOnTop, one variant of corn may stop growing. This has been manually special-cased.
        /// </para></remarks>
        /// <param name="property">A &lt;property&gt; element with name, value, [param1/2] attributes.</param>
        /// <param name="className">The class this property belongs to, or null for top-level properties.</param>
        private void AddProperty(XElement property, String className)
        {
            String name = property.Attribute("name")?.Value;
            String value = property.Attribute("value")?.Value;
            if (name == null) { throw new ArgumentException("The given property is missing a name."); }
            if (name == "CanPickup" && property.Attribute("param1") != null) {
                this.SetProperty("PickupTarget", property.Attribute("param1").Value);
            }
            if (className != null) {
                if (className == "RepairItems") {
                    this.SetProperty("RepairBlock.Item", name);
                    this.SetProperty("RepairBlock.ItemCount", value);
                } else {
                    this.SetProperty(className + "." + name, value);
                }
            } else {
                this.SetProperty(name, value);
            }
        }

        /// <summary>
        /// Represents the response to an event, possibly the dropping of a block.
        /// </summary>
        private class Drop
        {
            private BlockCollection _blockCollection;
            // This can be either a block or an item, or the special value '[recipe]'.
            private ObservableBase _item;
            // TODO: Subscribe to _item's PropertyChanged event to pick up the new name.
            private String _itemName;

            public Drop(Block block, XElement drop)
            {
                this._blockCollection = block.Collection as BlockCollection;
                this.Count = drop.Attribute("count")?.Value ?? "1";
                this.Name = drop.Attribute("name")?.Value;
                this.Probability = Convert.ToSingle(drop.Attribute("prob")?.Value ?? "1");
                this.StickChance = Convert.ToSingle(drop.Attribute("stick_chance")?.Value ?? "0");
                this.ToolCategory = drop.Attribute("tool_category")?.Value;
            }

            public String Count { get; set; }
            public String Name {
                get { return this._item?.Name ?? this._itemName; }
                set {
                    if (value != null) {
                        if (this._blockCollection.ContainsKey(value)) {
                            this._item = this._blockCollection[value];
                            this._itemName = this._item.Name;
                        } else if (this._blockCollection.Items.ContainsKey(value)) {
                            this._item = this._blockCollection.Items[value];
                            this._itemName = this._item.Name;
                        } else {
                            this._item = null;
                            this._itemName = value;
                        }
                    }
                }
            }
            public Single Probability { get; set; }
            public Single StickChance { get; set; }
            public String ToolCategory { get; set; }
        }
    }
}
