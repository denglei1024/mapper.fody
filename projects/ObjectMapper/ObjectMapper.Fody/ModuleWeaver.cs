using Fody;

namespace ObjectMapper.Fody;

public class ModuleWeaver: BaseModuleWeaver
{
    public override void Execute()
    {
        foreach (var type in ModuleDefinition.Types)
        {
            foreach (var method in type.Methods)
            {
                
            } 
        }
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        return Enumerable.Empty<string>();
    }
}
