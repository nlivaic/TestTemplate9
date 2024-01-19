using System.Threading.Tasks;

namespace TestTemplate9.Common.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveAsync();
    }
}