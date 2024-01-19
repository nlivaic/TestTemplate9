using System.Collections.Generic;

namespace TestTemplate9.Application.Sorting
{
    public class PropertyMappingValue
    {
        public PropertyMappingValue(string sourcePropertyName, IEnumerable<string> targetPropertyNames, bool revert)
        {
            SourcePropertyName = sourcePropertyName;
            TargetPropertyNames = targetPropertyNames;
            Revert = revert;
        }

        public string SourcePropertyName { get; set; }
        public IEnumerable<string> TargetPropertyNames { get; set; }
        public bool Revert { get; set; }
    }
}
