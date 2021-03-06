﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Digitalis;
using Digitalis.Features;
using Digitalis.Infrastructure;
using Digitalis.Models;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Raven.Client.Documents.Session;
using Specs.Infrastructure;
using Xunit;

namespace Specs.Features.AddNewEntry
{
    [Trait("Add New Entry", "Unauthorized and Empty Tags")]
    public class UnauthorizedAndEmptyTags : Fixture
    {
        private readonly HttpResponseMessage _response;
        private readonly CreateEntry.Command _newEntry;

        public UnauthorizedAndEmptyTags()
        {
            User user = new User
            {
                Email = "admin@app.com"
            };

            using var session = Store.OpenSession();
            session.Store(user);
            session.SaveChanges();

            var client = AuthClient(new Dictionary<string, string>
            {
                { "email", user.Email }
            });

            _newEntry = new CreateEntry.Command{ Tags = new string[] { }};

            _response = client.PostAsync("/entry",
                Serialize(_newEntry)).Result;

            WaitForIndexing(Store);
            WaitForUserToContinueTheTest(Store);
        }

        [Fact(DisplayName = "1. Forbidden is returned")]
        public void BadRequest()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact(DisplayName = "2. Zero entries is created in the database")]
        public void ZeroEntryCreated()
        {
            Store.OpenSession().Query<Entry>().Statistics(out QueryStatistics stats).ToList();

            stats.TotalResults.Should().Be(0);
        }

        [Fact(DisplayName = "3. Email is not sent")]
        public void EmailNotSent()
        {
            A.CallTo(() => Mailer
                    .SendMail(
                        A<string>.Ignored,
                        A<string>.Ignored,
                        A<string>.Ignored))
                .MustNotHaveHappened();
        }
    }
}
