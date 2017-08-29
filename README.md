# Mycro
Light-weight framework for building micro services

* Domain Driven Design using CQRSLite with optional use of Event Sourcing
* Built-in storage using LiteDB
* Automatic Discovery of Event/Command Handlers and WebApi Controllers
* Automatic Notification of domain events using SignalR
* Automatic generation of REST API documentation using Swagger
* Automatic hosting of SPA (e.g. Angular) web application using Nancy
* Scheduling using Quartz .NET

# Frameworks Overview

![frameworks used](https://github.com/pdeparcq/mycro/blob/master/Mycro/mycro.png)

# Getting Started

## Install nuget package

Make sure to change the target .NET framework of your project to 4.6

```
PM> Install-Package Televic.Mycro
```

## Your first running application

Create a new class derived from the framework Startup class


``` csharp
using Televic.Mycro.Web;

namespace TestMycro
{
    public class TestStartup : Startup
    {
        //By default application name will be same as Assembly name, so this is optional
        protected override string ApplicationName => "MyTestApp";
    }
}
```

Now use this startup class in your application to start the service. Here is an example for a console application;

``` csharp
using Televic.Mycro.Web;

namespace TestMycro
{
    class Program
    {
        static void Main(string[] args)
        {
            new Server<TestStartup>().Start();
        }
    }
}
```

This is it! You should now be able to run your application and see some logging. By default you can access the swagger ui at the following location: <http://localhost:9000/swagger>. This will be empty since we have no api controllers yet. Let's add one next.

## Adding an API controller using LiteDB

ASP .NET WebApi controllers are automatically detected. Here is an example that allows us to store and retrieve customers to and from the database:

``` csharp
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;
using LiteDB;

namespace TestMycro
{
    //This is a dummy customer model for demo purposes with some data annotations for validation
    public class CustomerModel
    {
        [Required(AllowEmptyStrings = false)]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string LastName { get; set; }
    }

    [RoutePrefix("api/customers")]
    public class CustomersController : ApiController
    {
        private readonly LiteRepository _repository;

        //LiteDB Repository will be automatically injected by AutoFac dependency injection framework
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
            if(ModelState.IsValid)
                _repository.Insert(model);
        }
    }
}
```
Now you should be able to test your new api using the swagger ui. By default the application will look for a connectionstring with name {ApplicationName}DB (in this case MyTestAppDB). If not found a default connection string will be used with database.db as filename for database.

