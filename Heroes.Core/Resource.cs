﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Heroes.Core
{
    public class Resource
    {
        private bool _isFile;

        private string _resName;

        private Type _typeInCorrectAssembly;

        string _assemblyName;
        System.Resources.ResourceManager _resManager;
        System.Globalization.CultureInfo _culture;

        public Resource(string resName, string assemblyName, System.Resources.ResourceManager resManager, System.Globalization.CultureInfo culture)
        {
            _assemblyName = assemblyName;
            _resManager = resManager;
            _culture = culture;

            this._resName = resName;   // do not convert to lowercase
            if (this._resName == string.Empty)
            {
                throw new ArgumentNullException("path", "The path to the file or resource cannot be null.");
            }

            if (!Path.IsPathRooted(this._resName))
            {
                _isFile = false; // Load from resouce...

                string resName2 = this._resName;

                // Attempt to find the resource requested
                bool found = false;
                System.Reflection.Assembly core = System.Reflection.Assembly.Load(assemblyName);

                foreach (string res in core.GetManifestResourceNames())
                {
                    if (string.Compare(res, resName2, true) == 0)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    string typeName = resName2;
                    _typeInCorrectAssembly = core.GetType(typeName);
                }
            }
            else
            {
                _isFile = true;
            }
        }

        public Stream GetStream()
        {
            if (_isFile)
            {
                return File.Open(_resName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else
            {
                //string resName2 = GetResourceName(this._resName);
                string resName2 = this._resName;

                if (_typeInCorrectAssembly == null)
                {
                    Stream str;
                    str = System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream(resName2);

                    if (str == null) // Attempt to find via ResourceManager
                    {
                        str = Find(Path.GetFileNameWithoutExtension(resName2), _resManager, _culture);
                    }

                    return str;
                }
                else
                {
                    return System.Reflection.Assembly.GetAssembly(_typeInCorrectAssembly).GetManifestResourceStream(resName2);
                }
            }
        }

        public static System.IO.UnmanagedMemoryStream Find(string name, System.Resources.ResourceManager resManager, System.Globalization.CultureInfo culture)
        {
            return resManager.GetStream(name, culture);
        }

        public bool IsFile
        {
            get { return _isFile; }
        }

    }
}
