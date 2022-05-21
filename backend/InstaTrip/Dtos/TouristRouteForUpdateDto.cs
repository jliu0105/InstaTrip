using InstaTrip.API.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InstaTrip.API.Dtos
{
    public class TouristRouteForUpdateDto : TouristRouteForManipulationDto
    {
        [Required(ErrorMessage = "update needed")]
        [MaxLength(1500)]
        public override string Description { get; set; }
    }
}
