using System.Collections.Generic;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Televic.Mycro.Documents
{
    public class ImportFileParamType : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var requestAttributes = apiDescription.GetControllerAndActionAttributes<SwaggerFormAttribute>();
            foreach (var attr in requestAttributes)
            {
                operation.parameters = new List<Parameter>
                {
                    new Parameter
                    {
                        description = attr.Description,
                        name = attr.Name,
                        @in = "formData",
                        required = true,
                        type = "file",
                    }
                };
                operation.consumes.Add("multipart/form-data");
            }
        }
    }
}
