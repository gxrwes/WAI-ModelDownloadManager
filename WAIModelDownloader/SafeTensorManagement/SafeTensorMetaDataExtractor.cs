using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Python.Runtime;

namespace WAIModelDownloader.SafeTensorManagement
{
    public class SafeTensorMetaDataExtractor
    {
        public SafeTensorMetaDataExtractor(string pythonPath)
        {
            InitializePythonEngine(pythonPath);
        }

        private void InitializePythonEngine(string pythonPath)
        {
            try
            {
                string pythonHome = pythonPath; // Base directory of the Python installation
                string pythonDll = System.IO.Path.Combine(pythonHome, "python311.dll"); // Path to the Python DLL

                if (!System.IO.File.Exists(pythonDll))
                {
                    throw new FileNotFoundException($"Python DLL not found at {pythonDll}");
                }

                Console.WriteLine($"Setting Runtime.PythonDLL to {pythonDll}");
                Runtime.PythonDLL = pythonDll; // Set the path to the Python DLL

                Console.WriteLine($"Setting PythonEngine.PythonHome to {pythonHome}");
                PythonEngine.PythonHome = pythonHome; // Set the Python home directory

                string libPath = System.IO.Path.Combine(pythonHome, "Lib");
                string sitePackagesPath = System.IO.Path.Combine(libPath, "site-packages");

                Console.WriteLine($"Lib path: {libPath}");
                Console.WriteLine($"Site-packages path: {sitePackagesPath}");

                if (!System.IO.Directory.Exists(libPath))
                {
                    throw new DirectoryNotFoundException($"Lib directory not found at {libPath}");
                }

                if (!System.IO.Directory.Exists(sitePackagesPath))
                {
                    throw new DirectoryNotFoundException($"Site-packages directory not found at {sitePackagesPath}");
                }

                PythonEngine.PythonPath = $"{pythonHome};{libPath};{sitePackagesPath}"; // Set the Python path

                Console.WriteLine("Initializing Python Engine");
                PythonEngine.Initialize(); // Initialize the Python engine
                Console.WriteLine("Python Engine Initialized Successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Python Engine: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }


        public Dictionary<string, object> ExtractMetadata(string filePath)
        {
            using (Py.GIL())
            {
                try
                {
                    dynamic sys = Py.Import("sys");
                    string scriptPath = AppDomain.CurrentDomain.BaseDirectory + @"SafeTensorManagement\python";
                    sys.path.append(scriptPath);

                    dynamic extract_metadata = Py.Import("extract_metadata");
                    dynamic metadata = extract_metadata.extract_metadata(filePath);

                    string json = metadata.ToString();
                    var metadataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                    return metadataDict;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
