using Fody;
using Mono.Cecil;

namespace TestWeaver.Fody;

// fody 负责协调各个 weaver 的执行
// 每个 weaver 都需要继承 BaseModuleWeaver
public class ModuleWeaver: BaseModuleWeaver
{
    override public void Execute()
    {
        // 这里是 weaver 的主要逻辑
        // 你可以在这里操作程序集的中间语言 (IL) 代码

        // 遍历模块中的类型
        foreach (var type in ModuleDefinition.Types)
        {
            // 查找需要修改的方法
            foreach (var method in type.Methods)
            {
                // 使用 Mono.Cecil 修改方法的 IL 代码
                var il = method.Body.GetILProcessor();
                // 在这里插入、修改或删除 IL 指令
            }
        }
    }

    override public IEnumerable<string> GetAssembliesForScanning()
    {
        // 返回需要扫描的程序集名称
        return new[] { "mscorlib", "System.Core" };
    }
}
