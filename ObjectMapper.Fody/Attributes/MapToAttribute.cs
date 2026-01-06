namespace ObjectMapper.Fody.Attributes;

/// <summary>
/// 定义映射到某个类型的属性。支持多个映射目标类型。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class MapToAttribute(Type destinationType) : Attribute
{
    /// <summary>
    /// 目标类型
    /// </summary>
    public Type DestinationType { get; } = destinationType;
}