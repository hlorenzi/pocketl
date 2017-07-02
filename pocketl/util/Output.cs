using System;


namespace pocketl.util
{
    public abstract class Output
    {
        protected int indentation = 0;
        protected int nextAlignColumn = 0;
        protected ConsoleColor color = ConsoleColor.White;


        public abstract void Write(string str);


        public void WriteLine(string str = "")
        {
            this.Write(str);
            this.Write("\n");
        }


        public void Write(string str, params object[] objs)
        {
            this.Write(String.Format(str, objs));
        }


        public void WriteLine(string str, params object[] objs)
        {
            this.WriteLine(String.Format(str, objs));
        }


        public void Indent()
        {
            this.indentation++;
        }


        public void Unindent()
        {
            this.indentation--;
        }


        public void SetColor(ConsoleColor color)
        {
            this.color = color;
        }


        public void AlignColumn(int column)
        {
            this.nextAlignColumn = column;
        }


        public int Indentation
        {
            get { return this.indentation; }
        }


        public abstract Output Cloned
        {
            get;
        }


        public Output Indented
        {
            get
            {
                var clone = this.Cloned;
                clone.Indent();
                return clone;
            }
        }


        public Output WithColor(ConsoleColor color)
        {
            var clone = this.Cloned;
            clone.SetColor(color);
            return clone;
        }
    }


    public class OutputConsole : Output
    {
        private int indentationWritten = 0;
        private int column = 0;


        public override void Write(string str)
        {
            var colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = this.color;

            foreach (var c in str)
            {
                if (c == '\n')
                {
                    Console.WriteLine();
                    this.indentationWritten = 0;
                    this.column = 0;
                    this.nextAlignColumn = 0;
                }
                else
                {
                    while (this.indentationWritten < this.indentation)
                    {
                        this.indentationWritten++;
                        this.column += 3;
                        Console.Write("   ");
                    }

                    if (this.column > this.nextAlignColumn && this.nextAlignColumn > 0)
                    {
                        Console.WriteLine();
                        this.column = 0;
                    }

                    while (this.column < this.nextAlignColumn)
                    {
                        this.column++;
                        Console.Write(" ");
                    }

                    Console.Write(c);
                    this.column++;
                    this.nextAlignColumn = 0;
                }
            }

            Console.ForegroundColor = colorBefore;
        }


        public override Output Cloned
        {
            get
            {
                return new OutputConsole
                {
                    column = this.column,
                    indentation = this.indentation,
                    indentationWritten = this.indentationWritten,
                    color = this.color,
                    nextAlignColumn = this.nextAlignColumn
                };
            }
        }
    }
}
