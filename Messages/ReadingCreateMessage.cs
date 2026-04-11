namespace SampleMauiMvvmApp.Messages
{
    public class ReadingCreateMessage : ValueChangedMessage<Reading>
    {
        public ReadingCreateMessage(Reading value) : base(value)
        {
        }
    }
}