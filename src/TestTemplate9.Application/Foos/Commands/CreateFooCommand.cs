﻿using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MassTransit;
using MediatR;
using TestTemplate9.Application.Questions.Queries;
using TestTemplate9.Common.Exceptions;
using TestTemplate9.Common.Interfaces;
using TestTemplate9.Core.Entities;
using TestTemplate9.Core.Events;

namespace TestTemplate9.Application.Questions.Commands
{
    public class CreateFooCommand : IRequest<FooGetModel>
    {
        public string Text { get; set; }

        private class CreateFooCommandHandler : IRequestHandler<CreateFooCommand, FooGetModel>
        {
            private readonly IPublishEndpoint _publishEndpoint;
            private readonly IRepository<Foo> _repository;
            private readonly IMapper _mapper;

            public CreateFooCommandHandler(
                IPublishEndpoint publishEndpoint,
                IRepository<Foo> repository,
                IMapper mapper)
            {
                _publishEndpoint = publishEndpoint;
                _repository = repository;
                _mapper = mapper;
            }

            public async Task<FooGetModel> Handle(CreateFooCommand request, CancellationToken cancellationToken)
            {
                if (await _repository.GetSingleAsync(f => f.Text == request.Text) != null)
                {
                    throw new BusinessException($"Cannot create {nameof(Foo)} with text {request.Text}.");
                }
                var foo = new Foo(request.Text);
                await _repository.AddAsync(foo);

                // sending to queue
                await _publishEndpoint.Publish<IFooCommand>(new { Text = foo.Text });

                // sending to topic
                await _publishEndpoint.Publish<IFooEvent>(new { Text = foo.Text });
                return _mapper.Map<FooGetModel>(foo);
            }
        }
    }
}
