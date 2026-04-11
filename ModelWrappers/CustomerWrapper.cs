namespace SampleMauiMvvmApp.ModelWrappers
{
    public partial class CustomerWrapper : ObservableObject
    {
        public CustomerWrapper(Customer customerModel)
        {
            if (customerModel != null)
            {
                Custnmbr = customerModel.CUSTNMBR;
                Custname = customerModel.CUSTNAME;
                #region
                Custclas = customerModel.CUSTCLAS;
                State = customerModel.STATE;
                Zip = customerModel.ZIP;
                Phone1 = (long)customerModel.PHONE1;
                AreaErf = customerModel.AreaErf;
                ModelTitle = customerModel.ModelTitle;
                #endregion
                Readings = new();
                customerModel.Readings?.ForEach(x =>
                    Readings.Add(new ReadingWrapper(x))
                );
            }
        }

        public CustomerWrapper(object item)
        {
            Item = item;
        }

        #region New Props
        public string Custnmbr { get; set; }

        [ObservableProperty]
        public string custname;

        [ObservableProperty]
        public string state;

        [ObservableProperty]
        public string zip;

        [ObservableProperty]
        public long phone1;

        [ObservableProperty]
        public string custclas;

        [ObservableProperty]
        public string modelTitle;

        [ObservableProperty]
        public string areaErf;

        #endregion

        [ObservableProperty]
        private bool isNew;

        [ObservableProperty]
        private bool isUpdated;

        public ObservableCollection<ReadingWrapper> Readings { get; set; }
        public object Item { get; }
    }
}