using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSample
{
    public class AppData
    {
        private const string _filePath = "data.json";
        private List<Device> _devices;
        public List<Device> Devices => _devices;

        public AppData() 
        {
            Read();
        }

        public List<Device> Read()
        {
            if (!File.Exists(_filePath))
            {
                FileUtils.WriteToJsonFile(_filePath, new List<Device>());    
            }
            _devices = FileUtils.ReadFromJsonFile<List<Device>>(_filePath);
            return _devices;
        }

        public void Write()
        {
            FileUtils.WriteToJsonFile(_filePath, _devices);
        }
    }
}
