using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities
{
    public class List
    {
        public class Query : IRequest<Result<List<ActivityDTO>>> { }

        public class Handler : IRequestHandler<Query, Result<List<ActivityDTO>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                _context = context;
                _mapper = mapper;
                _userAccessor = userAccessor;
            }

            public async Task<Result<List<ActivityDTO>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var activities = await _context.Activities
                        //using projection instead of Eager loading since it is more efficient and less computationally expensive
                        //only SQL queries fields we are specifically interested in
                    .ProjectTo<ActivityDTO>(_mapper.ConfigurationProvider, new {currentUsername = _userAccessor.GetUsername()})
                    .ToListAsync(cancellationToken);
                
                return Result<List<ActivityDTO>>.Success(activities);
            } 
        }
    }
}