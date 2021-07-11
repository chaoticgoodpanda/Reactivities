using System.Reflection.Metadata;
using System.Threading.Tasks;
using Application.Photos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class PhotosController : BaseApiController
    {
        //Have to get Add attribute [FromForm] to tell method where to find photo file
        [HttpPost]
        public async Task<IActionResult> Add([FromForm] Add.Command command)
        {
            return HandleResult(await Mediator.Send(command));
        } 
        
        //for deletion need to get id of the photo from the root parameter.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command {Id = id}));
        }
        
        //need to get id of photo we're setting to main
        //clearer to use /setMain here, not required
        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMain(string id)
        {
            return HandleResult(await Mediator.Send(new SetMain.Command {Id = id}));
        }
    }
}