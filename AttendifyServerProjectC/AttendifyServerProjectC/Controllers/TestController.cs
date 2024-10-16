using Microsoft.AspNetCore.Mvc;
using AttendifySharedProjectC.Models;

namespace AttendifyServerProjectC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        //[HttpPost]
        //public IActionResult ReceiveTestModel([FromBody] TestModel model)
        //{
        //    // Modify the incoming model and send it back
        //    model.Message = "Server received: " + model.Message;
        //    return Ok(model);
        //}

        private readonly ApplicationDbContext _context;

        private readonly ILogger<TestController> _logger;

        public TestController(ApplicationDbContext context, ILogger<TestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        //test 1, just to see if there is a connection between the server and the client   WORKS
        //[HttpPost]
        //public IActionResult ReceiveTestModel([FromBody] TestModel model)
        //{
        //    if (model == null)
        //    {
        //        return BadRequest("Invalid model");
        //    }

        //    model.Message = "Server received: " + model.Message;
        //    return Ok(model); // returns a JSON
        //}


        //test 2, to see if there also is a connection between the server and the sql database    WORKS
        [HttpPost]
        public IActionResult ReceiveTestModel([FromBody] TestModel model)
        {

            _logger.LogInformation("Hello World!");                 //this is how you use the logger. 
            Console.WriteLine("heeeee");                            //because the console. writeline does not show up in the debug console in the controllers
                                                                    //hence why the use of the logger :)

            var dbEntry = _context.TestTable.FirstOrDefault();      //NOTE TO SELF: look out for capitalization!
            if (dbEntry != null)
            {
                model.Message = $"Server received: {model.Message} | Database says: {dbEntry.Test}";
            }
            else
            {
                model.Message = $"Server received: {model.Message} | Database is empty";
            }

            return Ok(model);
        }
    }
}
