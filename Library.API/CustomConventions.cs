using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API
{
    public static class CustomConventions
    {
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]                 //This Allows any Method Starting with Insert as Prefix to use the Custom Conventions of this Insert Method.
        public static void Insert(
            [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Any)]                //This Allows any argument name and not limited to "model".
            [ApiConventionTypeMatch(ApiConventionTypeMatchBehavior.Any)]                //This Allows any type for the argument.
            object model)
        { 

        }
    }
}
