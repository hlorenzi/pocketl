namespace pocketl.pass
{
    public class TypeResolver
    {
        public static sema.Type Resolve(Context ctx, diagn.Reporter reporter, syn.Node node)
        {
            var type =
                ResolveStructure(ctx, reporter, node as syn.Node.TypeStructure) ??
                ResolvePointer(ctx, reporter, node as syn.Node.TypePointer) ??
                ResolveRefCounted(ctx, reporter, node as syn.Node.TypeRefCounted) ??
                ResolveTuple(ctx, reporter, node as syn.Node.TypeTuple) ??
                ResolveFunction(ctx, reporter, node as syn.Node.TypeFunction);

            if (type == null)
            {
                reporter.InternalError("node is not a type", new diagn.Caret(node.span));
                return new sema.Type.Error();
            }

            return type;
        }


        public static sema.Type ResolveStructure(Context ctx, diagn.Reporter reporter, syn.Node.TypeStructure node)
        {
            if (node == null)
                return null;

            var nodeName = node.name as syn.Node.Identifier;
            var name = nodeName.token.excerpt;

            var namespaceNode = ctx.names.Find(name);
            if (namespaceNode == null)
            {
                reporter.Error("unknown `" + name + "`", new diagn.Caret(nodeName.span));
                return new sema.Type.Error();
            }

            var namespaceItem = namespaceNode.item as sema.Namespace.Item.Def;
            var defStructure = ctx[namespaceItem.def] as sema.Def.Structure;
            if (defStructure == null)
            {
                reporter.Error("`" + name + "` is not a type", new diagn.Caret(nodeName.span));
                return new sema.Type.Error();
            }

            return new sema.Type.Structure { def = namespaceItem.def };
        }


        public static sema.Type ResolvePointer(Context ctx, diagn.Reporter reporter, syn.Node.TypePointer node)
        {
            if (node == null)
                return null;

            return new sema.Type.Pointer
            {
                mutable = node.mutable,
                innerType = Resolve(ctx, reporter, node.innerType)
            };
        }


        public static sema.Type ResolveRefCounted(Context ctx, diagn.Reporter reporter, syn.Node.TypeRefCounted node)
        {
            if (node == null)
                return null;

            return new sema.Type.RefCounted
            {
                mutable = node.mutable,
                innerType = Resolve(ctx, reporter, node.innerType)
            };
        }


        public static sema.Type ResolveTuple(Context ctx, diagn.Reporter reporter, syn.Node.TypeTuple node)
        {
            if (node == null)
                return null;

            var tuple = new sema.Type.Tuple();

            foreach (var elem in node.elements)
                tuple.types.Add(Resolve(ctx, reporter, elem));

            return tuple;
        }


        public static sema.Type ResolveFunction(Context ctx, diagn.Reporter reporter, syn.Node.TypeFunction node)
        {
            if (node == null)
                return null;

            var fn = new sema.Type.Function();

            foreach (var elem in node.parameters)
                fn.parameters.Add(Resolve(ctx, reporter, elem));

            if (node.returnType == null)
                fn.returnType = new sema.Type.Tuple();
            else
                fn.returnType = Resolve(ctx, reporter, node.returnType);

            return fn;
        }
    }
}
