namespace SampleMauiMvvmApp.Fakers
{
    public class ReadingFaker : Faker<Reading>
    {
        public ReadingFaker()
        {
            //RuleFor(x => x.CUSTOMER_NUMBER, x => "WB" + x.Finance.Random.String2(4, "3456789"));
            //RuleFor(x => x.ReadingDate, x => x.Date.Recent());
            RuleFor(x => x.PREVIOUS_READING, x => x.Random.Decimal(0, 00));
        }
    }
}