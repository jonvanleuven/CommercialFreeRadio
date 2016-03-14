using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CommercialFreeRadio.Impl
{
    public class UpnpInterface
    {
        private readonly string ip;

        public UpnpInterface(string ip)
        {
            if( string.IsNullOrEmpty(ip) )
                throw new NullReferenceException("Ip address is mandatory");
            this.ip = ip;
        }

        public void SetVolume(int volume)
        {
            SoapCall(
                "MediaRenderer/RenderingControl/Control",
                "urn:upnp-org:serviceId:RenderingControl#SetVolume",
                @"<ns0:SetVolume xmlns:ns0=""urn:schemas-upnp-org:service:RenderingControl:1""><InstanceID>0</InstanceID><Channel>Master</Channel><InstanceID>0</InstanceID><DesiredVolume>" + volume + "</DesiredVolume></ns0:SetVolume>");
        }

        public int GetVolume()
        {
            var xml = SoapCall(
                "MediaRenderer/RenderingControl/Control",
                "urn:upnp-org:serviceId:RenderingControl#GetVolume",
                @"<ns0:GetVolume xmlns:ns0=""urn:schemas-upnp-org:service:RenderingControl:1""><InstanceID>0</InstanceID><Channel>Master</Channel></ns0:GetVolume>");
            return ParseInt(GetElementValue(xml, "CurrentVolume")).Value;
        }

        public void SetAVTransportURI(string uri)
        {
            SoapCall(
                "MediaRenderer/AVTransport/Control",
                "urn:schemas-upnp-org:service:AVTransport:1#SetAVTransportURI",
                @"<u:SetAVTransportURI xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1""><InstanceID>0</InstanceID><CurrentURI>" + uri + "</CurrentURI><CurrentURIMetaData></CurrentURIMetaData></u:SetAVTransportURI>");
        }

        public void Play()
        {
            SoapCall(
                "MediaRenderer/AVTransport/Control",
                "urn:schemas-upnp-org:service:AVTransport:1#Play",
                @"<u:Play xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1""><InstanceID>0</InstanceID><Speed>1</Speed></u:Play>");
        }

        public void Stop()
        {
            SoapCall(
                "MediaRenderer/AVTransport/Control",
                "urn:schemas-upnp-org:service:AVTransport:1#Stop",
                @"<u:Stop xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1""><InstanceID>0</InstanceID><Speed>1</Speed></u:Stop>");
        }

        public GetMediaInfoResponse GetMediaInfo()
        {
            return new GetMediaInfoResponse(SoapCall(
                "MediaRenderer/AVTransport/Control",
                "urn:schemas-upnp-org:service:AVTransport:1#GetMediaInfo",
                @"<u:GetMediaInfo xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1""><InstanceID>0</InstanceID></u:GetMediaInfo>"));
        }

        public GetTransportInfoResponse GetTransportInfo()
        {
            return new GetTransportInfoResponse(SoapCall(
                "MediaRenderer/AVTransport/Control",
                "urn:schemas-upnp-org:service:AVTransport:1#GetTransportInfo",
                @"<u:GetTransportInfo xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1""><InstanceID>0</InstanceID></u:GetTransportInfo>"));
        }

        public GetPositionInfoResponse GetPositionInfo()
        {
            return new GetPositionInfoResponse(SoapCall(
                "MediaRenderer/AVTransport/Control",
                "urn:schemas-upnp-org:service:AVTransport:1#GetPositionInfo",
                @"<u:GetPositionInfo xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1""><InstanceID>0</InstanceID></u:GetPositionInfo>"));
        }

        private XElement SoapCall(string path, string soapAction, string bodyXml)
        {
            Logger.Debug("SoapCall: {0}", soapAction);
            var wr = WebRequest.Create("http://" + ip + ":1400/" + path);
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?><s:Envelope s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body>" + bodyXml + "</s:Body></s:Envelope>";
            wr.ContentType = "text/xml;charset=utf-8";
            wr.ContentLength = xml.Length;

            wr.Headers.Add("SOAPAction", soapAction);
            wr.Method = "POST";
            wr.GetRequestStream().Write(Encoding.UTF8.GetBytes(xml), 0, xml.Length);

            string soapResult;
            using (var webResponse = wr.GetResponse())
            {
                using (var rd = new StreamReader(webResponse.GetResponseStream()))
                {
                    soapResult = rd.ReadToEnd();
                }
            }
            var result = XDocument.Parse(soapResult).Elements().First().Elements().First().Elements().First(); //response node
            Logger.Debug(result);
            return result;
        }

        public class GetTransportInfoResponse
        {
            internal GetTransportInfoResponse(XElement xml)
            {
                CurrentTransportState = GetElementValue(xml, "CurrentTransportState");
                CurrentTransportStatus = GetElementValue(xml, "CurrentTransportStatus");
                CurrentSpeed = ParseInt(GetElementValue(xml, "CurrentSpeed") );
            }

            public int? CurrentSpeed { get; private set; }
            public string CurrentTransportState { get; private set; }
            public string CurrentTransportStatus { get; private set; }
        }

        public class GetMediaInfoResponse
        {
            internal GetMediaInfoResponse(XElement xml)
            {
                NrTracks = GetElementValue(xml, "NrTracks");
                MediaDuration = GetElementValue(xml, "MediaDuration");
                CurrentURI = GetElementValue(xml, "CurrentURI");
                NextURI = GetElementValue(xml, "NextURI");
                NextURIMetaData = GetElementValue(xml, "NextURIMetaData");
                PlayMedium = GetElementValue(xml, "PlayMedium");
                RecordMedium = GetElementValue(xml, "RecordMedium");
                WriteStatus = GetElementValue(xml, "WriteStatus");
            }
            public string NrTracks { get; private set; }
            public string MediaDuration { get; private set; }
            public string CurrentURI { get; private set; }
            public string NextURI { get; private set; }
            public string NextURIMetaData { get; private set; }
            public string PlayMedium { get; private set; }
            public string RecordMedium { get; private set; }
            public string WriteStatus { get; private set; }
        }

        public class GetPositionInfoResponse
        {
            internal GetPositionInfoResponse(XElement xml)
            {
                Track = GetElementValue(xml, "Track");
                TrackDuration = GetElementValue(xml, "TrackDuration");
                TrackMetaData = GetElementValue(xml, "TrackMetaData");
                TrackURI = GetElementValue(xml, "TrackURI");
                RelTime = GetElementValue(xml, "RelTime");
                AbsTime = GetElementValue(xml, "AbsTime");
                RelCount = GetElementValue(xml, "RelCount");
                AbsCount = GetElementValue(xml, "AbsCount");
            }
            public string Track { get; private set; }
            public string TrackDuration { get; private set; }
            public string TrackMetaData { get; private set; }
            public string TrackURI { get; private set; }
            public string RelTime { get; private set; }
            public string AbsTime { get; private set; }
            public string RelCount { get; private set; }
            public string AbsCount { get; private set; }
        }

        private static string GetElementValue(XElement xml, string elementName)
        {
            var node = xml.XPathSelectElement(elementName);
            return node == null ? null : node.Value;
        }

        private static int? ParseInt(string s)
        {
            int result;
            if (int.TryParse(s, out result))
                return result;
            return null;
        }
    }
}
