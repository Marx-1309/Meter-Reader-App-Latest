namespace SampleMauiMvvmApp.Interfaces
{
    public interface IBaseService
    {
        Task Init(DbContext dbContext);

        Task SeedData(DbContext dbContext);
    }
}