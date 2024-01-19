using System;
using TestTemplate9.Common.Base;
using TestTemplate9.Common.Exceptions;

namespace TestTemplate9.Core.Entities
{
    public class Foo : BaseEntity<Guid>
    {
        public Foo(string text)
        {
            Validate(text);
            Text = text;
        }

        private Foo()
        {
        }

        public string Text { get; private set; }

        private static void Validate(string text)
        {
            if (text.Length < 5)
            {
                throw new BusinessException($"Foo text must be at least 5 characters.");
            }
        }
    }
}
