using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class Create
    {
        //queries return data, commands do not, so there's no type in the IRequest
        public class Command : IRequest<Result<Unit>>
        {
            public Activity Activity { get; set; }

        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Activity).SetValidator(new ActivityValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;

            private readonly IUserAccessor _userAccessor;

            //bring in DataContext so we can persist our changes
            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            //<Unit> is an object MediatR provide but doesn't actually have any value -- just a way for MediatR to tell API action is finished.
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                //gets user from user object while using ASP.NET Identity
                var user = await _context.Users.FirstOrDefaultAsync(x => 
                    x.UserName == _userAccessor.GetUsername());
                
                //create new attendee from the user information obtained from var user variable.
                var attendee = new ActivityAttendee
                {
                    AppUser = user,
                    Activity = request.Activity,
                    IsHost = true
                };
                
                //add attendee to activity
                request.Activity.Attendees.Add(attendee);
                
                _context.Activities.Add(request.Activity);

                var result = await _context.SaveChangesAsync() > 0;
                
                if (!result) return Result<Unit>.Failure("Failure to create activity.");

                //equivalent to nothing, just telling Controller we're finished here.
                return Result<Unit>.Success(Unit.Value);
            }
        }

    }
}