using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using System.IO;
using System.Security.Cryptography;

namespace MTU.Updater
{
    public class MTUGenerator
    {
        string path;

        public Exception LastError { get; private set; }
        public Func<HashAlgorithm> Algorithm { get; set; }
        public Func<Stream, string> HashFunction { get; set; }

        public MTUGenerator(string path)
        {
            this.path = path;

            Algorithm = new Func<HashAlgorithm>(() => MD5.Create());
            HashFunction = BaseHashing;
        }

        string BaseHashing(Stream s)
        {
            using (var algo = Algorithm())
            {
                var buf = algo.ComputeHash(s);
                return BitConverter.ToString(buf).Replace("-", string.Empty);
            }
        }

        string CreateNode(XmlDocument doc, string path, string file)
        {
            var root = doc.DocumentElement;
            var node = root.AppendChild(doc.CreateElement("Update"));
            var ap = node.Attributes.Append(doc.CreateAttribute("Path"));
            ap.Value = file.Replace(path, string.Empty);

            if (ap.Value[0] == Path.DirectorySeparatorChar || ap.Value[0] == Path.AltDirectorySeparatorChar)
                ap.Value = ap.Value.Substring(1);

            using (var fs = File.OpenRead(file))
            {
                node.Attributes.Append(doc.CreateAttribute("Hash")).Value = HashFunction(fs);
                node.Attributes.Append(doc.CreateAttribute("Size")).Value = Convert.ToString(fs.Length);
            }

            return node.Attributes["Hash"].Value;
        }

        public bool Generate(string filename)
        {
            try
            {
                //if (!Directory.Exists(path))
                //return false;

                var path = this.path;
                if (path.Last() != Path.DirectorySeparatorChar)
                    path += Path.DirectorySeparatorChar;

                var dp = Path.GetDirectoryName(filename);
                if (!Directory.Exists(dp))
                    Directory.CreateDirectory(dp);

                var hp = Path.Combine(path, "Hashes") + Path.DirectorySeparatorChar;
                if (Directory.Exists(hp))
                    Directory.Delete(hp, true);
                Directory.CreateDirectory(hp);

                var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                //if (files.Length == 0)
                    //return false;

                var doc = new XmlDocument();
                doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", "yes"));

                var root = doc.AppendChild(doc.CreateElement("root"));
                foreach (var file in files)
                {
                    var hash = CreateNode(doc, path, file);
                    var hfp = string.Concat(file.Replace(path, hp), ".hash");
                    var hdp = Path.GetDirectoryName(hfp);
                    if (!Directory.Exists(hdp))
                        Directory.CreateDirectory(hdp);

                    using (var hs = File.CreateText(hfp))
                        hs.Write(hash);
                    CreateNode(doc, path, hfp);
                }

                if (File.Exists(filename))
                    File.Delete(filename);

                using (var fs = File.CreateText(filename))
                using (var writer = new XmlTextWriter(fs))
                {
                    writer.Formatting = Formatting.Indented;
                    doc.Save(writer);
                }

                return true;
            }
            catch (Exception ex)
            {
                LastError = ex;
                return false;
            }
        }
    }
}