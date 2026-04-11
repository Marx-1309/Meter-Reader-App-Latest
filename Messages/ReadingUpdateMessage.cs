namespace SampleMauiMvvmApp.Messages
{
    public class ReadingUpdateMessage : ValueChangedMessage<Reading>
    {
        public ReadingUpdateMessage(Reading value) : base(value)
        {
        }
    }
}