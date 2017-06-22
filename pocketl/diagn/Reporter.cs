using System;
using System.Collections.Generic;


namespace pocketl.diagn
{
    public interface Reporter
    {
        void InternalError(string descr, params Caret[] carets);
        void Error(string descr, params Caret[] carets);
    }


    public class Caret
    {
        public Span span;
        public bool primary;
        public string descr;


        public Caret(Span span, bool primary = true, string descr = null)
        {
            this.span = span;
            this.primary = primary;
            this.descr = descr;
        }
    }


    public class ReporterDefault : Reporter
    {
        List<Message> messages = new List<Message>();


        class Message
        {
            public MessageKind kind;
            public string descr;
            public List<Caret> carets;
        }


        enum MessageKind
        {
            InternalError,
            Error,
            Warning,
            Info,
            Lint
        }


        void Reporter.InternalError(string descr, params Caret[] carets)
        {
            this.messages.Add(new Message
            {
                kind = MessageKind.InternalError,
                descr = descr,
                carets = new List<Caret>(carets)
            });
        }


        void Reporter.Error(string descr, params Caret[] carets)
        {
            this.messages.Add(new Message
            {
                kind = MessageKind.Error,
                descr = descr,
                carets = new List<Caret>(carets)
            });
        }


        public void PrintToConsole(Context ctx)
        {
            var prevForegroundColor = Console.ForegroundColor;

            foreach (var msg in this.messages)
            {
                this.PrintMessageToConsole(ctx, msg);
                Console.WriteLine();
            }

            Console.ForegroundColor = prevForegroundColor;
        }


        void PrintMessageToConsole(Context ctx, Message msg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            // Print location.
            var primaryCaret = msg.carets.Find(c => c.primary && c.span.unit != null);
            if (primaryCaret != null)
            {
                var unit = ctx.units[primaryCaret.span.unit];
                var src = unit.ReadSource(ctx);

                var unitName = unit.name;
                var start = util.CharCounter.LineColumnOfIndex(src, primaryCaret.span.start);
                var end = util.CharCounter.LineColumnOfIndex(src, primaryCaret.span.end);
                Console.WriteLine("{0}:{1}:{2} {3}:{4}:",
                    unitName,
                    start.line + 1, start.column + 1,
                    end.line + 1, end.column + 1);
            }
            else
                Console.WriteLine("<unknown location>:");

            // Print description.
            ConsoleColor textColor = ConsoleColor.White;
            switch (msg.kind)
            {
                case MessageKind.InternalError:
                    textColor = ConsoleColor.Red;
                    Console.ForegroundColor = textColor;
                    Console.Write("internal compiler error: ");
                    break;
                case MessageKind.Error:
                    textColor = ConsoleColor.Red;
                    Console.ForegroundColor = textColor;
                    Console.Write("error: ");
                    break;
                case MessageKind.Warning:
                    textColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = textColor;
                    Console.Write("warning: ");
                    break;
                case MessageKind.Info:
                    textColor = ConsoleColor.Cyan;
                    Console.ForegroundColor = textColor;
                    Console.Write("note: ");
                    break;
                case MessageKind.Lint:
                    textColor = ConsoleColor.Magenta;
                    Console.ForegroundColor = textColor;
                    Console.Write("lint: ");
                    break;
            }

            Console.WriteLine(msg.descr);

            // Print annotated source code.
            var curCaret = 0;
            while (curCaret < msg.carets.Count)
            {
                // Collect consecutive carets with the same source unit
                // and ascending line numbers.
                var carets = new List<Caret>();
                carets.Add(msg.carets[curCaret]);
                curCaret++;

                while (curCaret < msg.carets.Count &&
                    msg.carets[curCaret].span.unit?.id == carets[0].span.unit?.id &&
                    msg.carets[curCaret].span.start >= carets[0].span.start)
                {
                    carets.Add(msg.carets[curCaret]);
                    curCaret++;
                }

                if (carets[0].span.unit == null)
                    continue;

                // Collect line numbers that will be printed.
                // Also collect surrounding lines for contextual aid.
                var unit = ctx.units[carets[0].span.unit];
                var src = unit.ReadSource(ctx);
                var srcLineCount = util.CharCounter.LineCount(src);
                var surrounding = 2;
                var lineSet = new HashSet<int>();

                foreach (var caret in carets)
                {
                    var start = util.CharCounter.LineColumnOfIndex(src, caret.span.start);
                    var end = util.CharCounter.LineColumnOfIndex(src, caret.span.end);

                    for (var line = start.line - surrounding; line <= end.line + surrounding; line++)
                        lineSet.Add(line);
                }

                var lines = new List<int>(lineSet);
                lines.Sort();

                // Print unit name.
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("   --> {0}", unit.name);
                Console.WriteLine("     |");

                // Print source code lines.
                var lastLinePrinted = (int?)null;
                foreach (var line in lines)
                {
                    if (line < 0 || line >= srcLineCount)
                        continue;

                    // Print line number.
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                    if (lastLinePrinted.HasValue && line != lastLinePrinted.Value + 1)
                        Console.WriteLine("    ...");

                    lastLinePrinted = line;

                    Console.Write("{0,4} | ", line + 1);

                    // Print line text.
                    var range = util.CharCounter.IndexRangeOfLine(src, line);

                    Console.ForegroundColor = ConsoleColor.White;
                    for (var i = range.start; i < range.end; i++)
                    {
                        // Add a space for spans of zero characters.
                        if (carets.Find(c => i == c.span.start && i == c.span.end) != null)
                            Console.Write(" ");

                        // Print char with special handling for whitespace.
                        switch (src[i])
                        {
                            case '\t': Console.Write("  "); break;
                            case '\r': Console.Write(""); break;
                            case '\n': Console.Write(""); break;
                            default: Console.Write(src[i]); break;
                        }
                    }

                    Console.WriteLine();

                    // Print line caret markings.
                    var hasAnyMarkings =
                        (carets.Find(c => c.span.end >= range.start && c.span.start < range.end) != null);

                    if (hasAnyMarkings)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("     | ");
                        Console.ForegroundColor = textColor;

                        for (var i = range.start; i < range.end; i++)
                        {
                            // Print marking for spans of zero characters.
                            var zeroCharCaret = carets.Find(c => i == c.span.start && i == c.span.end);
                            if (zeroCharCaret != null)
                                Console.Write(zeroCharCaret.primary ? "^" : "-");

                            // Print marking with special handling for whitespace.
                            var marking = " ";
                            var charCaret = carets.Find(c => i >= c.span.start && i < c.span.end);
                            if (charCaret != null)
                                marking = (charCaret.primary ? "^" : "-");

                            switch (src[i])
                            {
                                case '\t': Console.Write("{0}{0}", marking); break;
                                case '\r': Console.Write(""); break;
                                case '\n': Console.Write(""); break;
                                default: Console.Write("{0}", marking); break;
                            }
                        }

                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
