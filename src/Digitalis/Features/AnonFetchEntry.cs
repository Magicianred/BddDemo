﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Digitalis.Infrastructure.Mediatr;
using Digitalis.Models;
using FluentValidation;
using MediatR;
using Raven.Client.Documents.Session;

namespace Digitalis.Features
{
    public class AnonFetchEntry
    {
        public class Query : AnonRequest<Entry>
        {
            public string Id { get; set; }
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Query, Entry>
        {
            private readonly IAsyncDocumentSession _session;

            public Handler(IAsyncDocumentSession session)
            {
                _session = session;
            }

            public async Task<Entry> Handle(Query query, CancellationToken cancellationToken)
            {
                Entry entry = await _session.LoadAsync<Entry>(query.Id, cancellationToken);

                if (entry == null)
                    throw new KeyNotFoundException();

                return entry;
            }
        }
    }
}
