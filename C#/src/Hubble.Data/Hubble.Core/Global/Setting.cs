﻿/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Hubble.Framework.IO;
using Hubble.Framework.Serialization;

namespace Hubble.Core.Global
{
    /// <summary>
    /// Global setting
    /// </summary>
    [Serializable, System.Xml.Serialization.XmlRoot(Namespace = "http://hubbledotnet.codeplex.com")] 
    public class Setting
    {
        static private object _LockObj = new object();

        static private Setting _Config = null;

        static public Setting Config
        {
            get
            {
                lock (_LockObj)
                {
                    if (_Config == null)
                    {
                        Load();
                    }
                }

                return _Config;
            }

            set
            {
                _Config = value;
            }
        }

        const string FileName = "setting.xml";

        static public void Save()
        {
            string fileName = Path.AppendDivision(Path.ProcessDirectory, '\\') + FileName;

            using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create,
                 System.IO.FileAccess.ReadWrite))
            {
                XmlSerialization<Setting>.Serialize(Config, Encoding.UTF8, fs);
            }
        }

        static public void RemoveTableConfig(string tableDir)
        {
            Setting cfg = Config;
            lock (_LockObj)
            {
                cfg.Tables.Remove(new TableConfig(tableDir));
            }
        }

        static public bool TableExists(string tableDir)
        {
            Setting cfg = Config;
            lock (_LockObj)
            {
                return cfg.Tables.Contains(new TableConfig(tableDir));
            }
        }


        static public void Load()
        {
            string fileName = Path.AppendDivision(Path.ProcessDirectory, '\\') + FileName;

            if (System.IO.File.Exists(fileName))
            {
                try
                {
                    using (System.IO.FileStream fs = new System.IO.FileStream(FileName, System.IO.FileMode.Open,
                         System.IO.FileAccess.Read))
                    {
                        Config = XmlSerialization<Setting>.Deserialize(fs);
                    }
                }
                catch
                {
                    Config = new Setting();
                }
            }
            else
            {
                Config = new Setting();
            }
        }

        public Setting()
        {
            Directories = new Directories();
        }

        #region Public properties

        long _MemoryLimited = 800 * 1024 * 1024;

        public long MemoryLimited
        {
            get
            {
                lock (_LockObj)
                {
                    return _MemoryLimited;
                }
            }

            set
            {
                lock (_LockObj)
                {
                    _MemoryLimited = value;

                    if (_MemoryLimited < 1 * 1024 * 1024)
                    {
                        _MemoryLimited = 1 * 1024 * 1024;
                    }
                }
            }
        }

        Directories _Directories = new Directories();

        public Directories Directories
        {
            get
            {
                return _Directories;
            }

            set
            {
                _Directories = value;
            }
        }

        List<TableConfig> _Tables = new List<TableConfig>();

        public List<TableConfig> Tables
        {
            get
            {
                return _Tables;
            }

            set
            {
                _Tables = value;
            }
        }

        List<IQueryConfig> _IQuerys = new List<IQueryConfig>();

        /// <summary>
        /// IQuery exernal reference configuration 
        /// </summary>
        public List<IQueryConfig> IQuerys
        {
            get
            {
                return _IQuerys;
            }

            set
            {
                _IQuerys = value;
            }
        }

        List<IAnalyzerConfig> _IAnalyzers = new List<IAnalyzerConfig>();

        /// <summary>
        /// IAnalyzer exernal reference configuration 
        /// </summary>
        public List<IAnalyzerConfig> IAnalyzers
        {
            get
            {
                return _IAnalyzers;
            }

            set
            {
                _IAnalyzers = value;
            }
        }

        List<IDBAdapterConfig> _IDBAdapters = new List<IDBAdapterConfig>();

        /// <summary>
        /// IDBAdapter exernal reference configuration 
        /// </summary>
        public List<IDBAdapterConfig> IDBAdapters
        {
            get
            {
                return _IDBAdapters;
            }

            set
            {
                _IDBAdapters = value;
            }
        }


        #endregion
    }

    [Serializable]
    public class Directories
    {
        private string _LogDirectory;

        public string LogDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_LogDirectory))
                {
                    return Path.AppendDivision(Path.ProcessDirectory, '\\') + @"Log\";
                }

                return Path.AppendDivision(System.IO.Path.GetFullPath(_LogDirectory), '\\');
            }

            set
            {
                _LogDirectory = value;
            }
        }

        public Directories()
        {
            
        }
    }

    [Serializable]
    public class TableConfig
    {
        private string _Directory;

        public string Directory
        {
            get
            {
                return Path.AppendDivision(System.IO.Path.GetFullPath(_Directory), '\\');
            }

            set
            {
                _Directory = value;
            }
        }

        public TableConfig()
        {
        }

        public TableConfig(string dir)
        {
            Directory = dir;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return this.Directory.Trim().ToLower() == ((TableConfig)obj).Directory.Trim().ToLower();
        }

        public override int GetHashCode()
        {
            return this.Directory.GetHashCode();
        }
    }

    [Serializable]
    public class IQueryConfig : ExternalReference
    {
        [System.Xml.Serialization.XmlIgnore]
        protected override Type Interface
        {
            get 
            {
                return typeof(Query.IQuery);
            }
        }

        public IQueryConfig()
        {
        }

        public IQueryConfig(string assemblyFile)
        {
            base.AssemblyFile = assemblyFile;
        }
    }

    [Serializable]
    public class IAnalyzerConfig : ExternalReference
    {
        [System.Xml.Serialization.XmlIgnore]
        protected override Type Interface
        {
            get
            {
                return typeof(Analysis.IAnalyzer);
            }
        }

        public IAnalyzerConfig()
        {
        }

        public IAnalyzerConfig(string assemblyFile)
        {
            base.AssemblyFile = assemblyFile;
        }
    }

    [Serializable]
    public class IDBAdapterConfig : ExternalReference
    {
        [System.Xml.Serialization.XmlIgnore]
        protected override Type Interface
        {
            get
            {
                return typeof(DBAdapter.IDBAdapter);
            }
        }

        public IDBAdapterConfig()
        {
        }

        public IDBAdapterConfig(string assemblyFile)
        {
            base.AssemblyFile = assemblyFile;
        }
    }


}
