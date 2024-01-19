using TestTemplate9.Core.Entities;
using TestTemplate9.Core.Interfaces;

namespace TestTemplate9.Data.Repositories
{
    public class FooRepository : Repository<Foo>, IFooRepository
    {
        public FooRepository(TestTemplate9DbContext context)
            : base(context)
        {
        }
    }
}
