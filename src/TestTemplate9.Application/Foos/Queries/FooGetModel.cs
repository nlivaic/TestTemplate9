using System;
using AutoMapper;
using TestTemplate9.Core.Entities;

namespace TestTemplate9.Application.Questions.Queries
{
    public class FooGetModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; }

        public class FooGetModelProfile : Profile
        {
            public FooGetModelProfile()
            {
                CreateMap<Foo, FooGetModel>();
            }
        }
    }
}
