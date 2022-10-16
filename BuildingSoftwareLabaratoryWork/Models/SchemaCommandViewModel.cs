namespace BuildingSoftwareLabaratoryWork.Models;

public class SchemaCommandViewModel
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string TrueCaseScenario { get; set; }
    
    public string FalseCaseScenario { get; set; }
    
    public Guid BaseSchemaId { get; set; }
}