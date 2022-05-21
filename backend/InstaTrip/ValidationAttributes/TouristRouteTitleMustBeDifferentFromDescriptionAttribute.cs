using InstaTrip.API.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InstaTrip.API.ValidationAttributes
{
    public class TouristRouteTitleMustBeDifferentFromDescriptionAttribute: ValidationAttribute
    {
        protected override ValidationResult IsValid(
            object value, 
            ValidationContext validationContext
        )
        {
            var touristRouteDto = (TouristRouteForManipulationDto)validationContext.ObjectInstance;
            if (touristRouteDto.Title == touristRouteDto.Description)
            {
                return new ValidationResult(
                    "Name must be different from description",
                    new[] { "TouristRouteForCreationDto" }
                );
            }
            return ValidationResult.Success;
        }
    }
}
