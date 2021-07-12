using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Application.Comments;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class ChatHub : Hub
    {
        private readonly IMediator _mediator;

        public ChatHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        //difference between API controller is that user will be able to invoke methods inside this hub
        public async Task SendComment(Create.Command command)
        {
            //send the body of comment and activityID
            var comment = await _mediator.Send(command);
            
            //commentID shaped by CommentDTO send to anybody connected to the Hub including original author
            //because ActivityID is of type "Guid" have to convert to string w/ ToString()
            await Clients.Group(command.ActivityId.ToString())
                .SendAsync("ReceiveComment", comment.Value);
        }
        
        //want to connect client/uesrs to the group with the activityId
        //only need to do this when we connected; client is removed from any groups when we disconnect
        //new members to group receive comments based on this method
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            //get the key of the activityId
            var activityId = httpContext.Request.Query["activityId"];
            await Groups.AddToGroupAsync(Context.ConnectionId, activityId);
            var result = await _mediator.Send(new List.Query {ActivityId = Guid.Parse(activityId)});
            await Clients.Caller.SendAsync("LoadComments", result.Value);
        }
    }
}