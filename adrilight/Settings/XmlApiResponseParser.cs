﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows.Media;

namespace adrilight.Settings
{
    class XmlApiResponseParser
    {
        public static XmlApiResponse ParseApiResponse(string xml)
        {
            if (xml == null) return null;
            XmlApiResponse resp = new XmlApiResponse(); //XmlApiResponse object will contain parsed values
            try
            {
                XElement xe = XElement.Parse(xml);
                if (xe == null) return null;
                resp.Name = xe.Element("ds")?.Value;
                if (resp.Name == null) resp.Name = xe.Element("desc")?.Value; //try legacy XML element name (pre WLED 0.6.0)
                if (resp.Name == null) return null; //if we return at this point, parsing was unsuccessful (server likely not WLED device)

                string bri_s = xe.Element("ac")?.Value;
                if (bri_s == null) bri_s = xe.Element("act")?.Value; //try legacy XML element name (pre WLED 0.6.0)
                if (bri_s != null)
                {
                    int bri = 0;
                    Int32.TryParse(bri_s, out bri);
                    resp.Brightness = (byte)bri;
                    resp.IsOn = (bri > 0); //light is on if brightness > 0
                }

                byte r = 0, g = 0, b = 0;
                int counter = 0;
                foreach (var el in xe.Elements("cl"))
                {
                    int co = 0;
                    Int32.TryParse(el?.Value, out co);
                    switch (counter)
                    {
                        case 0: r = (byte)co; break;
                        case 1: g = (byte)co; break;
                        case 2: b = (byte)co; break;
                    }
                    counter++;
                }
                resp.LightColor = Color.FromRgb( r, g, b);
                return resp;
            }
            catch
            {
                //Exceptions here indicate unsuccessful parsing
            }
            return null;
        }
    }
}
