using System.Collections.Generic;
using TestTemplate9.Application.Sorting.Models;

namespace TestTemplate9.Application.Sorting
{
    public interface IPropertyMappingService
    {
        IEnumerable<SortCriteria> Resolve(BaseSortable sortableSource, BaseSortable sortableTarget);
    }
}
