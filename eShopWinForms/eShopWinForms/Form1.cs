﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Linq.Expressions;
using eShopWinForms.eShopServiceReference;

namespace eShopWinForms
{
    public partial class Form1 : Form
    {
        //Start service 
        ICatalogService service = new eShopServiceReference.CatalogServiceClient();
        //CatalogServiceMock service = new CatalogServiceMock();

        public Form1()
        {
            InitializeComponent();

            //Load Initial Data
            LoadCatalogData(service);

            // Adjust Column Display
            AllFilter();

            LoadTypeComboBox(service);
            LoadBrandComboBox(service);

            LoadCalendarProperties();
            LoadProductInputComboBox(service);
            LoadListBox(service);
            LoadListView();

        }


        private void catalogItemDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void LoadCatalogData(ICatalogService service)
        {
            IEnumerable<CatalogItem> items = service.GetCatalogItems();


            //var discountResponse = DiscountService.GetDiscount();
            //var discount = discountResponse.Result;

            //Get bound CatalogItem data
            var itemProperties = (from prop in typeof(CatalogItem).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                              let parameter = Expression.Parameter(typeof(CatalogItem), "obj")
                              let property = Expression.Property(parameter, prop)
                              let lambda = Expression.Lambda<Func<CatalogItem, object>>(Expression.Convert(property, typeof(object)), parameter).Compile()
                              select
                              new
                              {
                                  Getter = lambda,
                                  Name = prop.Name
                              }).ToArray();

            //Create image column
            DataGridViewImageColumn imgcol = new DataGridViewImageColumn();
            catalogItemDataGridView.Columns.Insert(0, imgcol);

            // Add columns to datagrid
            //int col_num = 0;
            foreach (var property in itemProperties)
            {
                string name = property.Name;
                catalogItemDataGridView.Columns.Add(name.ToString(), name);
                //col_num++;
            }

            
            // Load data into columns
            foreach (var catalogitem in items)
            {
                Image thumb = null;
                DataGridViewRow row = (DataGridViewRow)catalogItemDataGridView.Rows[0].Clone();
                int column = 1;
                foreach (var property in itemProperties)
                {
                    string name = property.Name;
                    object value = property.Getter(catalogitem);
                    row.Cells[column].Value = "" + value;

                    if (name.Equals("Picturefilename"))
                    {
                        //We can change this to relative path dont worry
                        string imagename = "C:\\git\\eShopModernizingWCFWinForms\\eShopWinForms\\eShopWinForms\\Assets\\Images\\Catalog\\" + value;
                        Image img = Image.FromFile(imagename);
                        thumb = img.GetThumbnailImage(192, 108, null, IntPtr.Zero);
                        //catalogItemDataGridView.Rows.Insert(0, thumb, 1);
                        row.Cells[0].Value = thumb;
                    }

                    //if (name.Equals("CatalogType"))
                    //{
                    //    CatalogType t = (CatalogType)value;
                    //    row.Cells[column].Value = t.Type;
                    //}
                    //catalogItemDataGridView.Rows.Insert(0, thumb, 1);
                    column++;
                }
                catalogItemDataGridView.Rows.Add(row);

            }

        }

        private void LoadTypeComboBox(ICatalogService service)
        {
            //Create IEnumerable List
            IEnumerable<CatalogType> types = service.GetCatalogTypes();

            // Bind combobox to dictionary
            Dictionary<string, string> typeDictionary = new Dictionary<string, string>();

            //Create "All" filter
            typeDictionary.Add("0", "All");

            // Add rest of type filters
            foreach (var catalogtype in types)
            {
                string idValue = catalogtype.Id.ToString();
                string typeValue = catalogtype.Type;
                typeDictionary.Add(idValue,typeValue);

            }

            //Load TypeComboBox
            catalogTypeComboBox.DataSource = new BindingSource(typeDictionary, null);
            catalogTypeComboBox.DisplayMember = "Value";
            catalogTypeComboBox.ValueMember = "Key";

        }

        private void LoadBrandComboBox(eShopServiceReference.ICatalogService service)
        {

            IEnumerable<CatalogBrand> brands = service.GetCatalogBrands();
            // Bind combobox to dictionary
            Dictionary<string, string> brandDictionary = new Dictionary<string, string>();

            //Create "All" filter
            brandDictionary.Add("0", "All");

            // Add rest of type filters
            foreach (var catalogbrand in brands)
            {
                string idValue = catalogbrand.Id.ToString();
                string brandValue = catalogbrand.Brand;
                brandDictionary.Add(idValue, brandValue);

            }

            //Load BrandComboBox
            catalogBrandComboBox.DataSource = new BindingSource(brandDictionary, null);
            catalogBrandComboBox.DisplayMember = "Value";
            catalogBrandComboBox.ValueMember = "Key";
        }

        private void AllFilter()
        {
            catalogItemDataGridView.Columns["Id"].Visible = false;
            catalogItemDataGridView.Columns["Picturefilename"].Visible = false;
            catalogItemDataGridView.Columns["CatalogBrandId"].Visible = false;
            catalogItemDataGridView.Columns["CatalogTypeId"].Visible = false;
            catalogItemDataGridView.Columns["CatalogBrand"].Visible = false;
            catalogItemDataGridView.Columns["CatalogType"].Visible = false;
            catalogItemDataGridView.Columns["ExtensionData"].Visible = false;
            catalogItemDataGridView.Columns["Name"].DisplayIndex = 1;
            catalogItemDataGridView.Columns["Description"].DisplayIndex = 2;
            catalogItemDataGridView.Columns["Price"].DisplayIndex = 3;

            for (int i = 0; i < catalogItemDataGridView.RowCount - 1; i++)
            {
                catalogItemDataGridView.Rows[i].Visible = true;
            }
        }

        private void Filter()
        {
            KeyValuePair<string, string> typeValue = (KeyValuePair<string, string>)catalogTypeComboBox.SelectedItem;
            string selectedTypeValue = typeValue.Key;

            KeyValuePair<string, string> brandValue = (KeyValuePair<string, string>)catalogBrandComboBox.SelectedItem;
            string selectedBrandValue = brandValue.Key;

            for (int i = 0; i < catalogItemDataGridView.RowCount - 1; i++)
            {
                var rowTypeValue = catalogItemDataGridView["CatalogTypeId", i].Value.ToString();
                var rowBrandValue = catalogItemDataGridView["CatalogBrandId", i].Value.ToString();

                if (selectedTypeValue == "0" && selectedBrandValue == "0")
                {
                    AllFilter();
                }

                else if (selectedTypeValue == "0" && selectedBrandValue == rowBrandValue)
                {
                    catalogItemDataGridView.Rows[i].Visible = true;
                }

                else if (selectedBrandValue == "0" && selectedTypeValue == rowTypeValue)
                {
                    catalogItemDataGridView.Rows[i].Visible = true;
                }

                else if (rowTypeValue == selectedTypeValue && rowBrandValue == selectedBrandValue)
                {
                    catalogItemDataGridView.Rows[i].Visible = true;
                }

                else
                {
                    catalogItemDataGridView.Rows[i].Visible = false;
                }
            }
        }

        private void LoadCalendarProperties()
        {
            monthCalendar1.MinDate = monthCalendar1.TodayDate;
            monthCalendar1.MaxSelectionCount = 1;

        }

        private void LoadListBox(ICatalogService service)
        {
            IEnumerable<CatalogItem> items = service.GetCatalogItems();
            IList<CatalogItem> itemsList = items.ToList();

            foreach (var catalogitem in itemsList)
            {
                //listBox1.Items.Add(String.Format("{0} {1}", catalogitem.Id, catalogitem.Name));
                listBox1.Items.Add(catalogitem.Id);
            }
            

        }

        private void LoadProductInputComboBox(ICatalogService service)
        {
            IEnumerable<CatalogItem> items = service.GetCatalogItems();
            IList<CatalogItem> itemsList = items.ToList();

            foreach (var catalogitem in itemsList)
            {
                productIdInput.Items.Add(catalogitem.Id);
            }

        }

        private void LoadListView()
        {
            listView1.Columns.Add("Date");
            listView1.Columns.Add("Id");
            listView1.Columns.Add("Availability");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime date = monthCalendar1.SelectionRange.Start.Date;
            int id = (int)listBox1.SelectedItem;
            int availability = service.GetAvailableStock(date, id);

            ListViewItem lvi = new ListViewItem(date.ToString());
            lvi.SubItems.Add(id.ToString());
            lvi.SubItems.Add(availability.ToString());
            listView1.Items.Add(lvi);

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void catalogTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Filter();

        }

        private void catalogBrandComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Filter();
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            
        }

        private void addAvailabilityButton_Click(object sender, EventArgs e)
        {
            int id = (int)productIdInput.SelectedItem;
            int quantity = int.Parse(quantityInput.Text);
            DateTime shipDate = Convert.ToDateTime(arrivalDateInput.Text);

            CatalogItemsStock shipment= new CatalogItemsStock();
            shipment.CatalogItemId = id;
            shipment.AvailableStock = quantity;
            shipment.Date = shipDate;

            service.CreateAvailableStock(shipment);
            MessageBox.Show("Shipment has been added to the database.");

            productIdInput.ResetText();
            quantityInput.Clear();
            arrivalDateInput.Clear();
        }
    }

}
