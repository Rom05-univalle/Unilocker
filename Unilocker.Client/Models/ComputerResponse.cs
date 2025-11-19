using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unilocker.Client.Models
{
    public class ComputerResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid Uuid { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public bool IsNewRegistration { get; set; }
        public ClassroomInfo? ClassroomInfo { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ClassroomInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BlockName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public int? Capacity { get; set; }
    }
}
