using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using TestTemplate9.Core.Entities;

namespace TestTemplate9.Application.Questions.Queries
{
    public class GetFooQuery : IRequest<FooGetModel>
    {
        public Guid Id { get; set; }

        private class GetFooQueryHandler : IRequestHandler<GetFooQuery, FooGetModel>
        {
            private readonly IMapper _mapper;

            public GetFooQueryHandler(IMapper mapper)
            {
                _mapper = mapper;
            }

            public Task<FooGetModel> Handle(GetFooQuery request, CancellationToken cancellationToken) =>
                Task.FromResult(_mapper.Map<FooGetModel>(new Foo("foofoo")));
        }
    }
}
