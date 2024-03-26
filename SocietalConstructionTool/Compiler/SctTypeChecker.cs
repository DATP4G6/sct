
using Antlr4.Runtime.Misc;

using Sct.Compiler.Exceptions;

namespace Sct.Compiler
{
    public class SctTypeChecker : SctBaseVisitor<SctType>
    {
        private readonly TypeTable _typeTable = new();

        public List<InvalidTypeException> Errors = new List<InvalidTypeException>();

        private readonly Ctable _ctable;

        public SctTypeChecker(Ctable ctable)
        {
            _ctable = ctable;
        }

        public override SctType VisitVariableDeclaration([NotNull] SctParser.VariableDeclarationContext context)
        {

            string typeName = context.type().GetText();
            SctType type = _typeTable.GetType(typeName) ?? throw new InvalidTypeException($"Type {typeName} does not exist");
            if (type == _typeTable.GetType("Predicate"))
            {
                Errors.Add(new InvalidTypeException("Variable cannot have a Predicate type"));
            }
            SctType expressionType = Visit(context.expression()); // This should call our overridden VisitExpression method with the expression context.
            if (type != expressionType)
            {
                Errors.Add(new InvalidTypeException($"Type mismatch: {type} != {expressionType}"));
            }
            return type;
        }
    }
}
