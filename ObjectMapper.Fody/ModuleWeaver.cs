using System.Reflection.Emit;
using Fody;
using Mono.Cecil;

namespace ObjectMapper.Fody;

public class ModuleWeaver: BaseModuleWeaver
{
    public override void Execute()
    {
        // 所有添加了MapFrom或MapTo特性的类，生成映射代码
        var processor = new MappingProcessor(ModuleDefinition);
        processor.Process();
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        return Enumerable.Empty<string>();
    }
}

public class MappingProcessor(ModuleDefinition moduleDefinition)
{
    private ModuleDefinition ModuleDefinition { get; } = moduleDefinition;

    public void Process()
    {
        // 扫描所有类型，查找标记了MapFrom或MapTo特性的类
        foreach (var type in ModuleDefinition.Types)
        {
            // 获取类的自定义特性
            var mapFromAttribute = type.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "MapFromAttribute");
            var mapToAttribute = type.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == "MapToAttribute");
            if (mapFromAttribute != null)
            {
                // 生成源类型到当前类型的映射代码
                GenerateMappingCode(type, mapFromAttribute, isFrom: true);
            }

            if (mapToAttribute != null)
            {
                // 生成当前类型到目标类型的映射代码
                GenerateMappingCode(type, mapToAttribute, isFrom: false);
            }
        }
    }

    private void GenerateMappingCode(TypeDefinition type, CustomAttribute mapAttribute, bool isFrom)
    {
        // 获取目标类型
        var targetType = (TypeReference)mapAttribute.ConstructorArguments[0].Value;
        var methodName = isFrom ? "MapFrom" : "MapTo";

        // 创建映射方法
        var method = new MethodDefinition(methodName,
            MethodAttributes.Public | MethodAttributes.Static,
            type);

        var parameterType = isFrom ? targetType : type;
        method.Parameters.Add(new ParameterDefinition("source", ParameterAttributes.None, parameterType));

        var ilProcessor = method.Body.GetILProcessor();

        // 创建目标对象实例
        ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, type.Methods.First(m => m.IsConstructor && !m.HasParameters)));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Stloc_0));

        // 遍历属性，生成赋值代码
        foreach (var property in type.Properties)
        {
            var sourceProperty = isFrom
                ? targetType.Resolve().Properties.FirstOrDefault(p => p.Name == property.Name)
                : type.Properties.FirstOrDefault(p => p.Name == property.Name);

            if (sourceProperty != null)
            {
                // 生成赋值代码
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_0));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, sourceProperty.GetMethod));
                ilProcessor.Append(ilProcessor.Create(OpCodes.Callvirt, property.SetMethod));
            }
        }

        // 返回目标对象
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldloc_0));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));

        // 将方法添加到类型中
        type.Methods.Add(method);
    }
}
