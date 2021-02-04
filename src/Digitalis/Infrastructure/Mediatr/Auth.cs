﻿using System.Linq;
using System.Security.Claims;
using Digitalis.Infrastructure.Guards;
using Digitalis.Models;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents.Session;

namespace Digitalis.Infrastructure.Mediatr
{
    public interface IAuth<in T>
    {
        public void Authenticate(T request);

        public void Authorize(T request);
    }

    public abstract class Auth<T> : IAuth<T>
    {
        protected readonly IHttpContextAccessor _ctx;
        protected readonly IDocumentSession Session;
        protected string Email;
        protected User User;

        protected Auth(IHttpContextAccessor ctx, IDocumentSession session)
        {
            _ctx = ctx;
            Session = session;
        }

        public void Authenticate(T request)
        {
            AuthenticationGuard.AgainstNull(_ctx.HttpContext?.User?.Identity);
            AuthenticationGuard.Affirm(_ctx.HttpContext?.User.Identity.IsAuthenticated);

            var ci = _ctx.HttpContext?.User.Identity as ClaimsIdentity;
            AuthenticationGuard.AgainstNull(ci);

            Claim? emailClaim = ci.Claims.SingleOrDefault(c => c.Type == "email") 
                                ?? ci.Claims.SingleOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            AuthenticationGuard.AgainstNull(emailClaim);

            Email = emailClaim.Value;
            AuthenticationGuard.AgainstNullOrEmpty(Email);

            User = Session.Query<User>().SingleOrDefault(x => x.Email == Email);
            if (User == null)
            {
                User = new User {Email = Email};
                Session.Store(User);
                Session.SaveChanges();
            }

            AuthenticationGuard.AgainstNull(User);
        }

        public virtual void Authorize(T request)
        {

        }
    }
}