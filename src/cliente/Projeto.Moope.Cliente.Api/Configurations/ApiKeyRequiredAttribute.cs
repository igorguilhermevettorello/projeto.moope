namespace Projeto.Moope.Cliente.Api.Configurations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ApiKeyRequiredAttribute : Attribute
    {
    }
}
