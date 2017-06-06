using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace plistParser
{
    class Program
    {
        static void Main(string[] args)
        {    

            List<PlistData> data = new List<PlistData>();
            //Reading contact from a plist file
            PList plist = new PList("Contacts.list");

            foreach (var item in plist.FirstOrDefault().Value)
            {
                var newData = new PlistData();
                var da = item;
                var fullName = item["FullName"];

                newData.FullName = fullName;

                foreach (var item1 in item["PhoneNumbers"])
                {
                    var phoneNumber = item1["Value"];
                    newData.PhoneNumber = phoneNumber;
                }

                data.Add(newData);
                var result = data.FirstOrDefault(d => d.FullName == fullName);
                if (result == null)
                {
                    //pData.Add(newData);
                }
            }


            StringBuilder sb = new StringBuilder();

            foreach (var item in data)
            {
                var myCard = new VCard
                {
                    FirstName = item.FullName,                   
                    Mobile = item.PhoneNumber
                };
                sb.Append(myCard.ToString());
            }
            
            using (var file = File.OpenWrite("C:\\contact.vcf"))
            using (var writer = new StreamWriter(file))
            {
                writer.Write(sb.ToString());
            }

            Console.ReadLine();
        }

        public class PlistData
        {
            public string FullName { get; set; }
            public string PhoneNumber { get; set; }
        }
    }

    public class VCard
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Organization { get; set; }
        public string JobTitle { get; set; }
        public string StreetAddress { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string CountryName { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string HomePage { get; set; }
        public byte[] Image { get; set; }
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("BEGIN:VCARD");
            builder.AppendLine("VERSION:2.1");
            // Name
            builder.AppendLine("N:;" + FirstName + ";;;");
            // Full name
            builder.AppendLine("FN:" + FirstName );
            // Address
            //builder.Append("ADR;HOME;PREF:;;");
            //builder.Append(StreetAddress + ";");
            //builder.Append(City + ";;");
            //builder.Append(Zip + ";");
            //builder.AppendLine(CountryName);
            // Other data
            //builder.AppendLine("ORG:" + Organization);
            //builder.AppendLine("TITLE:" + JobTitle);
            //builder.AppendLine("TEL;HOME;VOICE:" + Phone);
            builder.AppendLine("TEL;CELL:" + Mobile);
            //builder.AppendLine("URL;" + HomePage);
            //builder.AppendLine("EMAIL;PREF;INTERNET:" + Email);
            builder.AppendLine("END:VCARD");
            return builder.ToString();
        }
    }

    public class PList : Dictionary<string, dynamic>
    {
        public PList()
        {
        }

        public PList(string file)
        {
            Load(file);
        }

        public void Load(string file)
        {
            Clear();

            XDocument doc = XDocument.Load(file);
            XElement plist = doc.Element("plist");
            XElement dict = plist.Element("dict");

            var dictElements = dict.Elements();
            Parse(this, dictElements);
        }

        private void Parse(PList dict, IEnumerable<XElement> elements)
        {
            for (int i = 0; i < elements.Count(); i += 2)
            {
                XElement key = elements.ElementAt(i);
                XElement val = elements.ElementAt(i + 1);

                dict[key.Value] = ParseValue(val);
            }
        }

        private List<dynamic> ParseArray(IEnumerable<XElement> elements)
        {
            List<dynamic> list = new List<dynamic>();
            foreach (XElement e in elements)
            {
                dynamic one = ParseValue(e);
                list.Add(one);
            }

            return list;
        }

        private dynamic ParseValue(XElement val)
        {
            switch (val.Name.ToString())
            {
                case "string":
                    return val.Value;
                case "integer":
                    return int.Parse(val.Value);
                case "real":
                    return float.Parse(val.Value);
                case "true":
                    return true;
                case "false":
                    return false;
                case "dict":
                    PList plist = new PList();
                    Parse(plist, val.Elements());
                    return plist;
                case "array":
                    List<dynamic> list = ParseArray(val.Elements());
                    return list;
                default:
                    return false;
                    //throw new ArgumentException("Unsupported");
            }
        }
    }
}
