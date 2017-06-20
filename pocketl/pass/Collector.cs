﻿namespace pocketl.pass
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
            unit.semanticMap.def[node] = hDef;

            if (node.name != null)
            {
                var name = (node.name as syn.Node.Identifier).token.excerpt;
                var hNamespaceNode = ctx.names.FindOrReserve(name);
                hNamespaceNode.item = new sema.Namespace.Item.Def { def = hDef };
                ctx[hDef].hNamespaceNode = hNamespaceNode;
                unit.semanticMap.references[node] = hNamespaceNode;
                unit.semanticMap.references[node.name] = hNamespaceNode;
            }
        }


        private static void TryCollectFunctionDef(Context ctx, diagn.Reporter reporter, mod.Unit unit, syn.Node.FunctionDef node)
        {
            if (node == null)
                return;

            var hDef = ctx.defs.Reserve();
            ctx[hDef] = new sema.Def.Function();
            unit.semanticMap.def[node] = hDef;

            if (node.name != null)
            {
                var identifier = (node.name as syn.Node.Identifier).token.excerpt;
                var hNamespaceNode = ctx.names.FindOrReserve(identifier);
                hNamespaceNode.item = new sema.Namespace.Item.Def { def = hDef };
                ctx[hDef].hNamespaceNode = hNamespaceNode;
                unit.semanticMap.references[node] = hNamespaceNode;
                unit.semanticMap.references[node.name] = hNamespaceNode;
            }
        }
    }
}