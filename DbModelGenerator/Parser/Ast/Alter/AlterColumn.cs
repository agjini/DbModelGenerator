namespace DbModelGenerator.Parser.Ast.Alter
{
    public abstract class AlterColumnAction
    {
        public static AlterColumnAction AlterType(string type) => new AlterType(type);
        public static AlterColumnAction SetNotNull() => new SetNotNull();
        public static AlterColumnAction DropNotNull() => new DropNotNull();
    }

    public sealed class AlterType : AlterColumnAction
    {
        internal AlterType(string type)
        {
            Type = type;
        }

        public string Type { get; }
    }

    public sealed class SetNotNull : AlterColumnAction
    {
        internal SetNotNull()
        {
        }
    }

    public sealed class DropNotNull : AlterColumnAction
    {
        internal DropNotNull()
        {
        }
    }

    public sealed class AlterColumn : DdlAlterTableStatement
    {
        public AlterColumn(string column, AlterColumnAction alterColumnAction)
        {
            Column = column;
            AlterColumnAction = alterColumnAction;
        }

        public string Column { get; }
        public AlterColumnAction AlterColumnAction { get; }
    }
}