namespace pocketl.pass
{
    public class TypeResolver
    {
        public static sema.Type Resolve(Context ctx, diagn.Reporter reporter, syn.Node node)
        {
            var type = ResolveStructure(ctx, reporter, node as syn.Node.TypeStructure);

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
    }
}
