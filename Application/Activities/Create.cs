using System.Threading;
using System.Threading.Tasks;
using Domain;
using MediatR;
using Persistence;

namespace Application.Activities
{
    public class Create
    {
        //queries return data, commands do not, so there's no type in the IRequest
        public class Command : IRequest
        {
            public Activity Activity { get; set; }

        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;
            //bring in DataContext so we can persist our changes
            public Handler(DataContext context)
            {
                _context = context;
            }

            //<Unit> is an object MediatR provide but doesn't actually have any value -- just a way for MediatR to tell API action is finished.
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                //we're not touching the database at this point, just adding it in memory. So not need to use "AddAsync" method.
                _context.Activities.Add(request.Activity);

                await _context.SaveChangesAsync();

                //equivalent to nothing, just telling Controller we're finished here.
                return Unit.Value;
            }
        }

    }
}