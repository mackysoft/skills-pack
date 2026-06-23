namespace MackySoft.SkillsPack.Hosting.Cli.Common.Text;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class ContractLiteralAttribute : Attribute
{
    public ContractLiteralAttribute (string literal)
    {
        Literal = literal;
    }

    public string Literal { get; }
}
