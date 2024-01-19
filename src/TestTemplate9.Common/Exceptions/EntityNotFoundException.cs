using System;
using System.Collections.Generic;

namespace TestTemplate9.Common.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName, Guid id)
            : base($"Entity {entityName} with id {id} not found.")
        {
        }

        public EntityNotFoundException(string entityName, IEnumerable<Guid> ids)
            : base($"Entity {entityName} with ids {string.Join(", ", ids.ToString())} not found.")
        {
        }
    }
}