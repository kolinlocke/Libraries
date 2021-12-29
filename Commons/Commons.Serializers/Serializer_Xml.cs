using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Commons.Serializers
{
    public class Serializer_Xml<T_Obj> : Interface_Serializer<T_Obj>
    {
        public void SerializeToFile(T_Obj Source, String TargetPath)
        {
            if (File.Exists(TargetPath))
            { File.Delete(TargetPath); }

            FileInfo Fi_File = new FileInfo(TargetPath);
            if (!Fi_File.Directory.Exists)
            { Fi_File.Directory.Create(); }

            XmlWriterSettings Xs_Settings = this.GetXMLWriterSettings();

            using (FileStream Fs = new FileStream(TargetPath, FileMode.CreateNew, FileAccess.Write))
            {
                using (XmlWriter Xw = XmlWriter.Create(Fs, Xs_Settings))
                {
                    //XmlSerializerNamespaces XmlnsEmpty = new XmlSerializerNamespaces();
                    //XmlnsEmpty.Add("", "");

                    String XMLDefaultNamespace = this.GetDefaultXMLNamespace();
                    XmlSerializerNamespaces Xmlns = this.GetXMLNamespaces();
                    XmlSerializer Xs = new XmlSerializer(typeof(T_Obj), XMLDefaultNamespace);
                    Xs.Serialize(Xw, Source, Xmlns);
                }

                Fs.Close();
            }
        }

        public string SerializeToString(T_Obj Source)
        {
            String Serialized = "";

            XmlWriterSettings Xs_Settings = this.GetXMLWriterSettings();

            using (MemoryStream Stream = new MemoryStream())
            {
                using (XmlWriter Xw = XmlWriter.Create(Stream, Xs_Settings))
                {
                    String XMLDefaultNamespace = this.GetDefaultXMLNamespace();
                    XmlSerializerNamespaces Xmlns = this.GetXMLNamespaces();
                    XmlSerializer Xs = new XmlSerializer(typeof(T_Obj), XMLDefaultNamespace);
                    Xs.Serialize(Xw, Source, Xmlns);
                }

                //Gawa ni Marvin Pogi Verdera The Super Genius
                //Reset the Stream Position - Super Important
                Stream.Position = 0;

                using (StreamReader Reader = new StreamReader(Stream))
                {
                    Serialized = Reader.ReadToEnd();
                    Reader.Close();
                }

                Stream.Close();
            }

            return Serialized;
        }

        public T_Obj DeserializeFromFile(string SourcePath)
        {
            if (!File.Exists(SourcePath))
            { return default(T_Obj); }

            T_Obj Deserialized = default(T_Obj);

            using (FileStream Fs = new FileStream(SourcePath, FileMode.Open, FileAccess.Read))
            {
                String XMLDefaultNamespace = this.GetDefaultXMLNamespace();
                XmlSerializer Xs = new XmlSerializer(typeof(T_Obj), XMLDefaultNamespace);
                Deserialized = (T_Obj)Xs.Deserialize(Fs);
                Fs.Close();
            }

            return Deserialized;
        }

        public T_Obj DeserializeFromString(string Source)
        {
            MemoryStream Stream = new MemoryStream();
            StreamWriter Writer = new StreamWriter(Stream);
            Writer.Write(Source);
            Writer.Flush();

            String XMLDefaultNamespace = this.GetDefaultXMLNamespace();
            XmlSerializer Xs = new XmlSerializer(typeof(T_Obj), XMLDefaultNamespace);
            T_Obj Deserialized = (T_Obj)Xs.Deserialize(Stream);

            Writer.Close();
            Stream.Close();

            return Deserialized;
        }

        public virtual XmlWriterSettings GetXMLWriterSettings()
        {
            XmlWriterSettings Xs_Settings = new XmlWriterSettings();
            Xs_Settings.OmitXmlDeclaration = true;
            Xs_Settings.Indent = true;
            Xs_Settings.IndentChars = "\t";
            Xs_Settings.NewLineChars = "\r\n";
            Xs_Settings.NewLineHandling = NewLineHandling.Replace;

            return Xs_Settings;
        }

        public virtual XmlSerializerNamespaces GetXMLNamespaces()
        {
            XmlSerializerNamespaces XmlnsEmpty = new XmlSerializerNamespaces();
            XmlnsEmpty.Add("", "");

            return XmlnsEmpty;
        }

        public virtual String GetDefaultXMLNamespace()
        { return ""; }
    }
}
