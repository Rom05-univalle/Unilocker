using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unilocker.Client.Models
{
    public class RegisterComputerRequest
    {
        public string Name { get; set; } = string.Empty;
        public Guid Uuid { get; set; }
        public string? SerialNumber { get; set; }
        public string? Model { get; set; }
        public int ClassroomId { get; set; }
    }
}
