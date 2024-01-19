using System;

namespace TestTemplate9.Application.Sorting
{
    public class InvalidPropertyMappingException : Exception
    {
        public InvalidPropertyMappingException(string message)
            : base(message)
        {
        }
    }
}
