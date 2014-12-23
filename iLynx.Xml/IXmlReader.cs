//using System;
//using System.IO;

//namespace iLynx.Xml
//{
//    public interface IXmlReader
//    {
//        string ReadElementString(string elementName = "");
//        string ReadAttribute(string attributeName = "");
//        void ReadStartElement(string elementName = "");
//        void ReadEndElement();
//    }

//    public interface IXmlSegmentReader
//    {
//        string ReadStartSegment(PositioningReader reader);
//        bool ReadEndSegment(PositioningReader reader);
//    }

//    public class PositioningReader : TextReader
//    {
//        private readonly TextReader inner;
//        public PositioningReader(TextReader inner)
//        {
//            this.inner = inner;
//        }
//        public override void Close()
//        {
//            inner.Close();
//        }
//        public override int Peek()
//        {
//            return inner.Peek();
//        }
//        public override int Read()
//        {
//            var c = inner.Read();
//            if (c >= 0)
//                AdvancePosition((char)c);
//            return c;
//        }

//        private int lineNumber;
//        public int LineNumber { get { return lineNumber; } }

//        private int characterNumber;
//        public int CharacterNumber { get { return characterNumber; } }

//        private int matched;
//        private void AdvancePosition(char c)
//        {
//            if (Environment.NewLine[matched] == c)
//            {
//                matched++;
//                if (matched != Environment.NewLine.Length) return;
//                lineNumber++;
//                characterNumber = 0;
//                matched = 0;
//            }
//            else
//            {
//                matched = 0;
//                characterNumber++;
//            }
//        }
//    }

//    public class XmlElementReader : IXmlSegmentReader
//    {
//        #region Implementation of IXmlSegmentReader

//        public string ReadStartSegment(PositioningReader reader)
//        {
//            var startChar = (char) reader.Read();
//            if ('<' != startChar)
//                throw new InvalidDataException(
//                    string.Format("Expected start element '<', but got '{0}' - Line {1}, Character {2}",
//                                startChar,
//                                reader.LineNumber,
//                                reader.CharacterNumber));
//            var content = string.Empty;
//            var currentChar = (char) reader.Read();
//            while ()
//            return null;
//        }

//        public bool ReadEndSegment(PositioningReader reader)
//        {
//        }

//        #endregion
//    }

//    public class XmlReader : IXmlReader
//    {
//        #region Implementation of IXmlReader

//        public string ReadElementString(string elementName = "")
//        {

//            return null;
//        }

//        public string ReadAttribute(string attributeName = "")
//        {
//            return null;
//        }

//        public void ReadStartElement(string elementName = "")
//        {
//        }

//        public void ReadEndElement()
//        {
//        }

//        #endregion
//    }
//}
