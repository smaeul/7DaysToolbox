namespace BulkBlockEditor
{
    using Microsoft.Win32;
    using SDTD.Config;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;

    class MainViewModel : ViewModelBase
    {
        private String _defaultDataPath = @"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config";
        private String _filterMaterialString;
        private String _filterNameString;
        private Boolean _hasUnsavedChanges;
        private String _lastFileName;
        private String _setPropertyName;
        private String _setPropertyValue;

        public MainViewModel()
        {
            this.Items = new ItemCollection();
            this.Materials = new MaterialCollection();
            this.Blocks = new BlockCollection(this.Items, this.Materials);
            this.BlockView = CollectionViewSource.GetDefaultView(this.Blocks);
            this.BlockView.Filter = blockFilter;

            this.Exit = new RelayCommand(this.exitCommand);
            this.New = new RelayCommand(this.newCommand);
            this.Open = new RelayCommand(this.openCommand);
            this.Save = new RelayCommand(this.saveCommand);
            this.SaveAs = new RelayCommand(this.saveAsCommand);
            this.SetProperty = new RelayCommand(this.setPropertyCommand);
        }

        public event EventHandler RequestClose;

        public BlockCollection Blocks { get; }
        public ICollectionView BlockView { get; }
        public ICommand Exit { get; }
        public String FilterMaterialString {
            get { return this._filterMaterialString; }
            set {
                this.SetField(ref this._filterMaterialString, value);
                this.BlockView.Refresh();
            }
        }
        public String FilterNameString {
            get { return this._filterNameString; }
            set {
                this.SetField(ref this._filterNameString, value.ToLowerInvariant());
                this.BlockView.Refresh();
            }
        }
        public ItemCollection Items { get; }
        public MaterialCollection Materials { get; }
        public ICommand New { get; }
        public ICommand Open { get; }
        public ICommand Save { get; }
        public ICommand SaveAs { get; }
        public ICommand SetProperty { get; }
        public String SetPropertyName {
            get { return this._setPropertyName; }
            set { this.SetField(ref this._setPropertyName, value); }
        }
        public String SetPropertyValue {
            get { return this._setPropertyValue; }
            set { this.SetField(ref this._setPropertyValue, value); }
        }

        private Boolean blockFilter(Object parameter)
        {
            Block block = parameter as Block;
            Boolean matchesMaterial = false, matchesName = false;
            if (String.IsNullOrEmpty(this._filterMaterialString) ||
                block.GetProperty("Material") == this._filterMaterialString) {
                matchesMaterial = true;
            }
            if (String.IsNullOrEmpty(this._filterNameString) ||
                block.Name.ToLowerInvariant().Contains(this._filterNameString)) {
                matchesName = true;
            }
            return matchesMaterial && matchesName;
        }

        private void exitCommand(Object parameter)
        {
            if (this.shouldAbandonChanges()) {
                this.RequestClose?.Invoke(this, new EventArgs());
            }
        }

        private void newCommand(Object parameter)
        {
            if (this.shouldAbandonChanges()) {
                this.Blocks.Clear();
                this.Items.Clear();
                this.Materials.Clear();
                this._hasUnsavedChanges = false;
            }
        }

        private void openCommand(Object parameter)
        {
            if (this.shouldAbandonChanges()) {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "XML Files|*.xml";
                if (this._lastFileName != null) {
                    open.InitialDirectory = Path.GetDirectoryName(this._lastFileName);
                }
                if (open.ShowDialog() == true) {
                    this._hasUnsavedChanges = false;
                    this.newCommand(null);
                    this.Items.Load(this._defaultDataPath + Path.DirectorySeparatorChar + "items.xml");
                    this.Materials.Load(this._defaultDataPath + Path.DirectorySeparatorChar + "materials.xml");
                    this.Blocks.Load(open.FileName);
                    this._lastFileName = open.FileName;
                }
            }
        }

        private void saveAsCommand(Object parameter)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "XML Files|*.xml";
            if (this._lastFileName != null) {
                save.InitialDirectory = Path.GetDirectoryName(this._lastFileName);
            }
            if (save.ShowDialog() == true) {
                this._lastFileName = save.FileName;
                this.saveCommand(null);
            }
        }

        private void saveCommand(Object parameter)
        {
            if (this._lastFileName != null) {
                this.Blocks.Save(this._lastFileName);
                this._hasUnsavedChanges = false;
            } else {
                this.saveAsCommand(null);
            }
        }

        private void setPropertyCommand(Object parameter)
        {
            IList selectedItems = parameter as IList;
            foreach (Block block in selectedItems) {
                block.SetProperty(this._setPropertyName, this._setPropertyValue);
            }
            this._hasUnsavedChanges = true;
        }

        private Boolean shouldAbandonChanges()
        {
            if (this._hasUnsavedChanges) {
                MessageBoxResult answer = MessageBox.Show(
                    "Do you want to abandon your unsaved changes?",
                    "Confirmation",
                    MessageBoxButton.OKCancel);
                if (answer == MessageBoxResult.Cancel) {
                    return false;
                }
            }
            return true;
        }
    }
}
