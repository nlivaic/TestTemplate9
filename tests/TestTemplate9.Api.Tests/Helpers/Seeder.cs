using System.Collections.Generic;
using TestTemplate9.Core.Entities;
using TestTemplate9.Data;

namespace TestTemplate9.Api.Tests.Helpers
{
    public static class Seeder
    {
        public static void Seed(this TestTemplate9DbContext ctx)
        {
            ctx.Foos.AddRange(
                new List<Foo>
                {
                    new ("Text 1"),
                    new ("Text 2"),
                    new ("Text 3")
                });
            ctx.SaveChanges();
        }
    }
}
