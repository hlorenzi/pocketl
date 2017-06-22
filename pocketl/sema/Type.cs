using System.Collections.Generic;


namespace pocketl.sema
{
    public abstract class Type
    {
        public virtual string PrintableName(Context ctx)
        {
            return "<?>";
        }


        public class Error : Type
        {
            public override string PrintableName(Context ctx)
            {
                return "<error>";
            }
        }


        public class Structure : Type
        {
            public H<Def> def;


            public override string PrintableName(Context ctx)
            {
                return ctx.names.PrintableFullKeyOf(ctx[def].hNamespaceNode);
            }
        }


        public class Tuple : Type
        {
            public List<Type> types = new List<Type>();
        }


        public class Function : Type
        {
            public Type returntype;
            public List<Type> args = new List<Type>();
        }
    }
}
