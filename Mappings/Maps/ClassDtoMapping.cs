namespace SampleMauiMvvmApp.Mappings.Maps
{
    public class ClassDtoMapping : Profile
    {
        public ClassDtoMapping()
        {
            CreateMap<Reading, ReadingDto>().ReverseMap();
            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<Reading, UpdateReadingDto>().ReverseMap();
            CreateMap<Reading, ReadingMedia>().ReverseMap();
            CreateMap<ImageSyncDto, ReadingMedia>().ReverseMap();
        }
    }
}