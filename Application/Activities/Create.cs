using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Domain;
using FluentValidation;
using MediatR;
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
            //bring in DataContext so we can persist our changes
            public Handler(DataContext context)
            {
                _context = context;
            }

            //<Unit> is an object MediatR provide but doesn't actually have any value -- just a way for MediatR to tell API action is finished.
            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                //we're not touching the database at this point, just adding it in memory. So not need to use "AddAsync" method.
                _context.Activities.Add(request.Activity);

                var result = await _context.SaveChangesAsync() > 0;
                
                if (!result) return Result<Unit>.Failure("Failure to create activity.");

                //equivalent to nothing, just telling Controller we're finished here.
                return Result<Unit>.Success(Unit.Value);
            }
        }

    }
}