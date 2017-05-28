namespace pocketl.util
{
    public static class CharCounter
    {
        public struct LineColumn
        {
            public int line;
            public int column;
        }


        public struct Range
        {
            public int start;
            public int end;
        }


        public static int LineCount(string str)
        {
            var lines = 1;
            foreach (var c in str)
            {
                if (c == '\n')
                    lines++;
            }
            return lines;
        }


        public static LineColumn LineColumnOfIndex(string str, int index)
        {
            var line = 0;
            var column = 0;

            for (var i = 0; i < index && i < str.Length; i++)
            {
                if (str[i] == '\n')
                {
                    line += 1;
                    column = 0;
                }
                else
                    column += 1;
            }

            return new LineColumn { line = line, column = column };
        }


        public static Range IndexRangeOfLine(string str, int line)
        {
            var lineCount = 0;
            var lineStart = 0;

            while (lineCount < line && lineStart < str.Length)
            {
                lineStart++;

                if (str[lineStart - 1] == '\n')
                    lineCount++;
            }

            var lineEnd = lineStart;
            while (lineEnd < str.Length)
            {
                lineEnd++;

                if (str[lineEnd - 1] == '\n')
                    break;
            }

            return new Range { start = lineStart, end = lineEnd };
        }
    }
}
