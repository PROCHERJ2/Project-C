using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendifySharedProjectC.Models
{
    public class Event
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Column("BannerImage")]
        public byte[]? Image { get; set; }  

        public ICollection<EventDay> EventDays { get; set; }
        public ICollection<EventParticipant> Participants { get; set; }
    }

    public class EventDay
    {
        [Key]
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public DateTime Date { get; set; }  //day
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public Event Event { get; set; }
    }

    public class EventParticipant
    {
        [Key]
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string UserId { get; set; }
        public DateTime JoinDate { get; set; }

        public Event Event { get; set; }
    }
}
