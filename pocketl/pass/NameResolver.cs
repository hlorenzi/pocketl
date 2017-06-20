namespace pocketl.pass
{
    public class NameResolver
    {
        public static void Resolve(Context ctx, diagn.Reporter reporter, H<mod.Unit> hUnit)
        {
            var unit = ctx[hUnit];
            ResolveNode(ctx, reporter, unit, unit.ast);
        }


        private static void ResolveNode(Context ctx, diagn.Reporter reporter, mod.Unit unit, syn.Node node)
        {
            TryResolveIdentifier(ctx, reporter, unit, node as syn.Node.Identifier);

            foreach (var child in node.Children())
                ResolveNode(ctx, reporter, unit, child);
        }


        private static void TryResolveIdentifier(Context ctx, diagn.Reporter reporter, mod.Unit unit, syn.Node.Identifier node)
        {
            if (node == null)
                return;

            if (unit.semanticMap.references.ContainsKey(node))
                return;

            var identifier = node.token.excerpt;
            var hNamespaceNode = ctx.names.Find(identifier);
            if (hNamespaceNode != null)
                unit.semanticMap.references[node] = hNamespaceNode;
        }
    }
}
