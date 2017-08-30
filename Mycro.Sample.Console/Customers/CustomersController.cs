using System.Collections.Generic;
using System.Web.Http;
using LiteDB;

namespace Mycro.Sample.Console.Customers
{
    [RoutePrefix("api/customers")]
    public class CustomersController : ApiController
    {
        private readonly LiteRepository _repository;

        public CustomersController(LiteRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Route("")]
        public IEnumerable<CustomerModel> GetAllCustomers()
        {
            return _repository.Query<CustomerModel>().ToEnumerable();
        }

        [HttpPost]
        [Route("")]
        public void AddCustomer(CustomerModel model)
        {
            if (ModelState.IsValid)
                _repository.Insert(model);
        }
    }
}