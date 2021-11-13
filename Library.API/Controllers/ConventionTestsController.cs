//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace Library.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    //[ApiConventionType(typeof(DefaultApiConventions))]            //Api Conventions applied to the Entire Controller only. Note that ProducesResponseTypeAttribute values in Startup.cs  override these default convention types
//    [ApiConventionType(typeof(CustomConventions))]                  //Applying the Custom Api Conventions to the Entire Controller only. 
//    public class ConventionTestsController : ControllerBase
//    {
//        // GET: api/<ConventionTestsController>
//        [HttpGet]        
//        public IEnumerable<string> Get()
//        {
//            return new string[] { "value1", "value2" };
//        }

//        // GET api/<ConventionTestsController>/5
//        [HttpGet("{id}")]
//        //[ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))] //Api Conventions applied to the Entire Controller only. Note that ProducesResponseTypeAttribute values in Startup.cs  override these default convention types
//        public string Get(int id)
//        {
//            return "value";
//        }

//        // POST api/<ConventionTestsController>
//        [HttpPost]
//        //[ApiConventionMethod(typeof(CustomConventions),nameof(CustomConventions.Insert))]           //Using a Custom Cretaed Convention
//        public void InsertTest([FromBody] string value)
//        {
//        }

//        // PUT api/<ConventionTestsController>/5
//        [HttpPut("{id}")]
//        public void Put(int id, [FromBody] string value)
//        {
//        }

//        // DELETE api/<ConventionTestsController>/5
//        [HttpDelete("{id}")]
//        public void Delete(int id)
//        {
//        }
//    }
//}
