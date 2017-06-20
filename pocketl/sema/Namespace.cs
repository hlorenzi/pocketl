using System;
using System.Collections.Generic;
using System.Linq;


namespace pocketl.sema
{
    public class Namespace
    {
        public class Node
        {
            public Node parent;
            public Dictionary<string, Node> children = new Dictionary<string, Node>();
            public string name;
            public Item item;
        }


        public abstract class Item
        {
            public class Def : Item
            {
                public H<sema.Def> def;
            }
        }


        Node root;


        public Namespace()
        {
            this.root = new Node();
        }


        public Node Set(Item item, params string[] keys)
        {
            var node = this.Find(keys);
            node.item = item;
            return node;
        }


        public Node Find(params string[] keys)
        {
            var current = this.root;

            for (var i = 0; i < keys.Length; i++)
            {
                if (current.children.TryGetValue(keys[i], out var node))
                    current = node;

                else
                    return null;
            }

            return current;
        }


        public Node FindOrReserve(params string[] keys)
        {
            var current = this.root;

            for (var i = 0; i < keys.Length; i++)
            {
                if (current.children.TryGetValue(keys[i], out var node))
                    current = node;

                else
                {
                    var newNode = new Node();
                    newNode.parent = current;
                    newNode.name = keys[i];
                    current.children.Add(keys[i], newNode);
                    current = newNode;
                }
            }

            return current;
        }


        public string[] FullKeyOf(Node node)
        {
            var keys = new List<string>();

            while (node.name != null)
            {
                keys.Insert(keys.Count, node.name);
                node = node.parent;
            }

            return keys.ToArray();
        }


        public string PrintableFullKeyOf(Node node)
        {
            var keys = this.FullKeyOf(node);
            return String.Join(".", keys);
        }


        public void PrintToConsole(Context ctx)
        {
            this.PrintNodeToConsole(ctx, this.root);
        }


        private void PrintNodeToConsole(Context ctx, Node node, int indent = 0)
        {
            Console.Write(new string(' ', indent * 3));
            Console.Write(node.name ?? "::root::");

            if (node.item != null)
            {
                Console.Write(" ");

                var itemDef = node.item as Item.Def;
                if (itemDef != null)
                    Console.Write("<def#" + itemDef.def.id + "> ");
            }

            Console.WriteLine();

            foreach (var child in node.children)
                PrintNodeToConsole(ctx, child.Value, indent + 1);
        }
    }
}
