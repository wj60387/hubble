﻿using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.IO;

namespace Hubble.Core.Store
{
    class IndexWriter
    {
        string _HeadFilePath;
        string _IndexFilePath;

        System.IO.FileStream _HeadFile;
        System.IO.FileStream _IndexFile;

        private string HeadFilePath
        {
            get
            {
                return _HeadFilePath;
            }
        }

        private string IndexFilePath
        {
            get
            {
                return _IndexFilePath;
            }
        }

        public IndexWriter(int serial, string path, string fieldName)
        {
            _HeadFilePath = Path.AppendDivision(path, '\\') + 
                string.Format("{0:D4}{1}.hdx", serial, fieldName);
            _IndexFilePath = Path.AppendDivision(path, '\\') +
                string.Format("{0:D4}{1}.idx", serial, fieldName);

            _HeadFile = new System.IO.FileStream(_HeadFilePath, System.IO.FileMode.Create,
                 System.IO.FileAccess.Write);
            _IndexFile = new System.IO.FileStream(_IndexFilePath, System.IO.FileMode.Create,
                 System.IO.FileAccess.Write);


        }

        public long AddWordAndDocList(string word, List<Entity.DocumentPositionList> docList, out long length)
        {
            long position = _IndexFile.Position;
            byte[] wordBuf = Encoding.UTF8.GetBytes(word);

            _HeadFile.Write(BitConverter.GetBytes(wordBuf.Length + sizeof(long) + sizeof(long)), 0, sizeof(int)); //Size
            _HeadFile.Write(BitConverter.GetBytes(position), 0, sizeof(long)); //doc position in index file

         
            foreach (Entity.DocumentPositionList doc in docList)
            {
                Hubble.Framework.Serialization.MySerialization<Entity.DocumentPositionList>.Serialize(_IndexFile, doc);
            }

            _IndexFile.WriteByte(0);
            //_IndexFile.WriteByte(0);

            length = _IndexFile.Position - position;

            _HeadFile.Write(BitConverter.GetBytes(length), 0, sizeof(long)); //word index data length

            _HeadFile.Write(wordBuf, 0, wordBuf.Length);

            return position;
        }

        public void Close()
        {
            _HeadFile.Close();
            _IndexFile.Close();
        }
    }
}