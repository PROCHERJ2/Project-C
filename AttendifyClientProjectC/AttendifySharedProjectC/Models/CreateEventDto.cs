using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendifySharedProjectC.Models
{
    public class CreateEventDto                                               
    {                                                                         
        public Guid Id { get; set; }                                            
        public string Name { get; set; }                                      
        public string Description { get; set; }                                 
        public string? Image { get; set; }  
        public List<EventDayDto> EventDays { get; set; }
    }

    public class EventDayDto                                                   
    {
        public DateTime Day { get; set; }
        public string StartTime { get; set; }  // sadly, cant use TimeOnly when sending to controller.
        public string EndTime { get; set; }    

    }
}
//The Events model can be used instead, but for now lets leave it like this
//since we might wanna abstract for security reasons. plus using a separate
//dto instead of the model is better because if we change the db then it 
//wont break the dto. so communication between client and server will
//be fine. Also, Events model uses TimeOnly, which is a pain in the ass because
//you cant really send it from the server to the client and vise versa, you
//have to convert it first.