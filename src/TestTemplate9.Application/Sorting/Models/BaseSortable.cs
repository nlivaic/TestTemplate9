using System;
using System.Collections.Generic;

namespace TestTemplate9.Application.Sorting.Models
{
    public abstract class BaseSortable
    {
        public abstract Type ResourceType { get; }
        public abstract IEnumerable<SortCriteria> SortBy { get; set; }
    }
}
