namespace SDTD.Config
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    /// <summary>
    /// Represents an item with modifiable properties, expressible as an XML element.
    /// </summary>
    interface IElement
    {
        void AddProperty(XElement property);
        String GetProperty(String name);
        Boolean SetProperty(String name, String value);
        XElement ToXElement();
    }

    /// <summary>
    /// Represents an observable object with a unique, immutable name and (optionally) ID.
    /// </summary>
    public abstract class ObservableBase : INotifyPropertyChanged
    {
        protected ObservableBase(UInt32? id, String name)
        {
            if (name == null) { throw new ArgumentNullException(nameof(name)); }
            this.ID = id;
            this.Name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public UInt32? ID { get; }
        public String Name { get; }

        protected void OnPropertyChanged([CallerMemberName] String propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected Boolean SetField<U>(ref U field, U value, [CallerMemberName] String propertyName = null)
        {
            if (EqualityComparer<U>.Default.Equals(field, value)) {
                return false;
            }
            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// Represents an element in an <c>ObservableBaseCollection</c> of the same type.
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    public abstract class ObservableBase<T> : ObservableBase
        where T : ObservableBase<T>
    {
        protected static String _elementName = typeof(T).Name.ToLowerInvariant();

        protected ObservableBase(ObservableBaseCollection<T> collection, UInt32? id, String name) : base(id, name)
        {
            if (collection == null) { throw new ArgumentNullException(nameof(collection)); }
            this.Collection = collection;
        }

        public ObservableBaseCollection<T> Collection { get; }
    }
}

namespace System.Runtime.CompilerServices
{
    sealed class CallerMemberNameAttribute : Attribute { }
}
