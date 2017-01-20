using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;


namespace ACABUS_Control_de_operacion
{
    class Configuration
    {
        public Dictionary<string, string> informationKVR()
        {
            Dictionary<string, string> _information = new Dictionary<string, string>();

            _information.Add("KVR020101", "172.17.1.41");
            _information.Add("KVR020102", "172.17.1.42");
            _information.Add("KVR020201", "172.17.2.41");
            _information.Add("KVR020202", "172.17.2.42");
            _information.Add("KVR020301", "172.17.3.41");
            _information.Add("KVR020302", "172.17.3.42");
            _information.Add("KVR020303", "172.17.3.43");
            _information.Add("KVR020304", "172.17.3.44");
            _information.Add("KVR020401", "172.17.4.41");
            _information.Add("KVR020402", "172.17.4.42");
            _information.Add("KVR020501", "172.17.5.41");
            _information.Add("KVR020502", "172.17.5.42");
            _information.Add("KVR020601", "172.17.6.41");
            _information.Add("KVR020602", "172.17.6.42");
            _information.Add("KVR020701", "172.17.7.41");
            _information.Add("KVR020702", "172.17.7.42");
            _information.Add("KVR020801", "172.17.8.41");
            _information.Add("KVR020802", "172.17.8.42");
            _information.Add("KVR020803", "172.17.8.43");
            _information.Add("KVR020901", "172.17.9.41");
            _information.Add("KVR020902", "172.17.9.42");
            _information.Add("KVR021001", "172.17.10.41");
            _information.Add("KVR021002", "172.17.10.42");
            _information.Add("KVR021101", "172.17.11.41");
            _information.Add("KVR021102", "172.17.11.42");
            _information.Add("KVR021201", "172.17.12.41");
            _information.Add("KVR021202", "172.17.12.42");
            _information.Add("KVR021203", "172.17.12.43");
            _information.Add("KVR021204", "172.17.12.44");
            _information.Add("KVR021205", "172.17.12.45");
            _information.Add("KVR021206", "172.17.12.46");
            _information.Add("KVR021207", "172.17.12.47");
            _information.Add("KVR021208", "172.17.12.48");
            _information.Add("KVR021301", "172.17.13.41");
            _information.Add("KVR021302", "172.17.13.42");
            _information.Add("KVR021401", "172.17.14.41");
            _information.Add("KVR021402", "172.17.14.42");
            _information.Add("KVR021501", "172.17.15.41");
            _information.Add("KVR021502", "172.17.15.42");
            _information.Add("KVR021601", "172.17.16.41");
            _information.Add("KVR021701", "172.17.17.41");
            _information.Add("KVR021702", "172.17.17.42");
            _information.Add("KVR021801", "172.17.18.41");
            _information.Add("KVR021802", "172.17.18.42");
            _information.Add("KVR021901", "172.17.19.41");
            _information.Add("KVR021902", "172.17.19.42");
            _information.Add("KVR022001", "172.17.20.41");
            _information.Add("KVR022002", "172.17.20.42");
            _information.Add("KVR022101", "172.17.21.41");
            _information.Add("KVR022102", "172.17.21.42");
            _information.Add("KVR022201", "172.17.22.41");
            _information.Add("KVR022202", "172.17.22.42");
            _information.Add("KVR022301", "172.17.23.41");
            _information.Add("KVR022302", "172.17.23.42");
            _information.Add("KVR022401", "172.17.24.41");
            _information.Add("KVR022402", "172.17.24.42");
            _information.Add("KVR022501", "172.17.25.41");
            _information.Add("KVR022502", "172.17.25.42");
            _information.Add("KVR022601", "172.17.26.41");

            return _information;
        }

        public Dictionary<string, string> informationSQL()
        {
            Dictionary<string, string> _configuration = new Dictionary<string, string>();

            _configuration.Add("Server", null);
            _configuration.Add("Port", "5432");
            _configuration.Add("Id", "postgres");
            _configuration.Add("Password", "4c4t3k");
            _configuration.Add("Database", "SITM");

            return _configuration;
        }

        public void _informationKVR()
        {

        }
    }
}
