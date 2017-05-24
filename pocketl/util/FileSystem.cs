using System.Collections.Generic;


namespace pocketl.util
{
    public interface FileSystem
    {
        string Read(string filename);
    }


    public class FileSystemMock : FileSystem
    {
        Dictionary<string, string> files = new Dictionary<string, string>();


        public void Add(string filename, string contents)
        {
            this.files.Add(filename, contents);
        }


        string FileSystem.Read(string filename)
        {
            return this.files[filename];
        }
    }
}
