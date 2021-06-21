using System;
using System.ComponentModel.DataAnnotations;

namespace APIGateway.Core.RoutingService
{
    public class RoutingRequest
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string SharedIdentificator { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public string SessionId { get; set; }

        public string OperatorId { get; set; }

        public DateTime? Processed { get; set; }
    }
}
