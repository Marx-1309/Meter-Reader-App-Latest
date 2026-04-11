using SampleMauiMvvmApp.Mappings.Dto_s;
using System.Text.Json.Serialization;

namespace SampleMauiMvvmApp.Mappings.Dto_s
{
    public class CustomerDto
    {
        public string CUSTMBR { get; set; }
        public string CUSTNAME { get; set; }
        public string CUSTCLAS { get; set; }
        public string STATE { get; set; }
        public string ZIP { get; set; }
    }
}

[JsonSerializable(typeof(List<CustomerDto>))]
internal sealed partial class CustomerDtoContext : JsonSerializerContext
{
}