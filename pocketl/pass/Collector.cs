namespace pocketl.pass
{
    public class Collector
    {
        public static void Collect(Context ctx, diagn.Reporter reporter, H<mod.Unit> hUnit)
        {
            var unit = ctx[hUnit];
            CollectAtNode(ctx, reporter, unit, unit.ast);
        }


        private static void CollectAtNode(Context ctx, diagn.Reporter reporter, mod.Unit unit, syn.Node node)
        {
            TryCollectStructureDef(ctx, reporter, unit, node as syn.Node.StructureDef);
            TryCollectFunctionDef(ctx, reporter, unit, node as syn.Node.FunctionDef);

            foreach (var child in node.Children())
                CollectAtNode(ctx, reporter, unit, child);
        }


        private static void TryCollectStructureDef(Context ctx, diagn.Reporter reporter, mod.Unit unit, syn.Node.StructureDef node)
        {
            if (node == null)
                return;

            var hDef = ctx.defs.Reserve();
            ctx[hDef] = new sema.Def.Structure();
            ctx[hDef].spanDef = node.span;
            unit.semanticMap.def[node] = hDef;

            if (node.name != null)
            {
                var identifierToken = (node.name as syn.Node.Identifier).token;
                ctx[hDef].spanDefName = identifierToken.span;

                var namespaceNode = ctx.names.FindOrReserve(identifierToken.excerpt);
                if (namespaceNode.item != null)
                    ReportDuplicate(ctx, reporter, identifierToken.span, namespaceNode);

                else
                {
                    namespaceNode.item = new sema.Namespace.Item.Def { def = hDef };
                    ctx[hDef].namespaceNode = namespaceNode;
                    unit.semanticMap.references[node] = namespaceNode;
                    unit.semanticMap.references[node.name] = namespaceNode;
                }
            }
        }


        private static void TryCollectFunctionDef(Context ctx, diagn.Reporter reporter, mod.Unit unit, syn.Node.FunctionDef node)
        {
            if (node == null)
                return;

            var hDef = ctx.defs.Reserve();
            ctx[hDef] = new sema.Def.Function();
            ctx[hDef].spanDef = node.span;
            unit.semanticMap.def[node] = hDef;

            if (node.name != null)
            {
                var identifierToken = (node.name as syn.Node.Identifier).token;
                ctx[hDef].spanDefName = identifierToken.span;

                var namespaceNode = ctx.names.FindOrReserve(identifierToken.excerpt);
                if (namespaceNode.item != null)
                    ReportDuplicate(ctx, reporter, identifierToken.span, namespaceNode);

                else
                {
                    namespaceNode.item = new sema.Namespace.Item.Def { def = hDef };
                    ctx[hDef].namespaceNode = namespaceNode;
                    unit.semanticMap.references[node] = namespaceNode;
                    unit.semanticMap.references[node.name] = namespaceNode;
                }
            }
        }


        private static void ReportDuplicate(Context ctx, diagn.Reporter reporter, diagn.Span newSpan, sema.Namespace.Node originalNode)
        {
            var originalSpan = (diagn.Span)null;

            var originalItemDef = originalNode.item as sema.Namespace.Item.Def;
            if (originalItemDef != null)
                originalSpan = ctx[originalItemDef.def].spanDefName;

            if (originalSpan != null)
            {
                reporter.Error(
                    "duplicate definition of `" + ctx.names.PrintableFullKeyOf(originalNode) + "`",
                    new diagn.Caret(newSpan), new diagn.Caret(originalSpan, false));
            }
            else
            {
                reporter.Error(
                    "duplicate definition of `" + ctx.names.PrintableFullKeyOf(originalNode) + "`",
                    new diagn.Caret(newSpan));
            }
        }
    }
}
