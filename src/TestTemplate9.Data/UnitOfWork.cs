using System.Threading.Tasks;
using TestTemplate9.Common.Interfaces;

namespace TestTemplate9.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TestTemplate9DbContext _dbContext;

        public UnitOfWork(TestTemplate9DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> SaveAsync()
        {
            if (_dbContext.ChangeTracker.HasChanges())
            {
                return await _dbContext.SaveChangesAsync();
            }
            return 0;
        }
    }
}