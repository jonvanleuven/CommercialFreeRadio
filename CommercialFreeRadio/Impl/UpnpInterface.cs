using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CommercialFreeRadio.Impl
{
    public class UpnpInterface
    {
        public UpnpInterface(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                throw new NullReferenceException("Ip address is mandatory");
            Ip = ip;
        }

        public string Ip { get; private set; }

        public GetZoneGroupStateStateResponse GetZoneGroupState()
        {
            var response = SoapCall(
                "ZoneGroupTopology/Control",
                "urn:upnp-org:serviceId:ZoneGroupTopology#GetZoneGroupState",
                @"<ns0:GetZoneGroupState xmlns:ns0=""urn:schemas-upnp-org:service:ZoneGroupTopology:1""><ZoneGroupState></ZoneGroupState></ns0:GetZoneGroupState>");
            var xml = (XElement) XDocument.Parse(GetElementValue(response, "ZoneGroupState")).FirstNode;
            return new GetZoneGroupStateStateResponse(xml);
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

        public void SetAVTransportURI(string uri, string uriMetadata = null)
        {
            SoapCall(
                "MediaRenderer/AVTransport/Control",
                "urn:schemas-upnp-org:service:AVTransport:1#SetAVTransportURI",
                @"<u:SetAVTransportURI xmlns:u=""urn:schemas-upnp-org:service:AVTransport:1""><InstanceID>0</InstanceID><CurrentURI>" + XmlEscape(uri) + "</CurrentURI><CurrentURIMetaData>" + XmlEscape(uriMetadata ?? string.Empty) +
                "</CurrentURIMetaData></u:SetAVTransportURI>");
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

        public static string XmlEscape(string unescaped)
        {
            if (unescaped == null)
                return unescaped;
            var node = new XmlDocument().CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }

        private XElement SoapCall(string path, string soapAction, string bodyXml)
        {
            Logger.Debug("SoapCall calling action: {0}", soapAction);
            var wr = WebRequest.Create("http://" + Ip + ":1400/" + path);
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?><s:Envelope s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body>" + bodyXml + "</s:Body></s:Envelope>";
            wr.ContentType = "text/xml;charset=utf-8";
            wr.ContentLength = xml.Length;

            wr.Headers.Add("SOAPAction", soapAction);
            wr.Method = "POST";
            wr.GetRequestStream().Write(Encoding.UTF8.GetBytes(xml), 0, xml.Length);

            using (var webResponse = wr.GetResponse())
            {
                using (var rd = new StreamReader(webResponse.GetResponseStream()))
                {
                    var result = XDocument.Parse(rd.ReadToEnd()).Elements().First().Elements().First().Elements().First(); //response node
                    Logger.Debug("SoapCall result: " + result.ToString().Replace("\n", ""));
                    return result;
                }
            }
        }

        public class GetTransportInfoResponse
        {
            internal GetTransportInfoResponse(XElement xml)
            {
                CurrentTransportState = GetElementValue(xml, "CurrentTransportState");
                CurrentTransportStatus = GetElementValue(xml, "CurrentTransportStatus");
                CurrentSpeed = ParseInt(GetElementValue(xml, "CurrentSpeed"));
            }

            public int? CurrentSpeed { get; private set; }
            public string CurrentTransportState { get; private set; }
            public string CurrentTransportStatus { get; private set; }
        }

        public class GetZoneGroupStateStateResponse
        {
            internal GetZoneGroupStateStateResponse(XElement xml)
            {
                ZoneGroups = xml.Elements().Select(g => new ZoneGroup
                {
                    Coordinator = g.Attribute("Coordinator").Value,
                    Id = g.Attribute("ID").Value,
                    ZoneGroupMembers = ((XElement) g).Elements().Select(gm => new ZoneGroupMember
                    {
                        Id = gm.Attribute("UUID").Value,
                        Location = gm.Attribute("Location").Value,
                        Name = gm.Attribute("ZoneName").Value,
                    }).ToList()
                }).ToList();
            }

            private GetZoneGroupStateStateResponse()
            {
            }

            public IList<ZoneGroup> ZoneGroups { get; private set; }
        }

        public class ZoneGroup
        {
            public string Coordinator { get; internal set; }
            public string Id { get; internal set; }

            public IList<ZoneGroupMember> ZoneGroupMembers { get; internal set; }

        }

        public class ZoneGroupMember
        {
            public string Id { get; internal set; }
            public string Location { get; internal set; }
            public string Name { get; internal set; }
            public string Ip { get { return Location.Substring("http://".Length, Location.IndexOf(":1400/") - 1 - ":1400/".Length); } }
        }
    
        public class GetMediaInfoResponse
        {
            internal GetMediaInfoResponse(XElement xml)
            {
                NrTracks = GetElementValue(xml, "NrTracks");
                MediaDuration = GetElementValue(xml, "MediaDuration");
                CurrentURI = GetElementValue(xml, "CurrentURI");
                CurrentURIMetaData = GetElementValue(xml, "CurrentURIMetaData");
                NextURI = GetElementValue(xml, "NextURI");
                NextURIMetaData = GetElementValue(xml, "NextURIMetaData");
                PlayMedium = GetElementValue(xml, "PlayMedium");
                RecordMedium = GetElementValue(xml, "RecordMedium");
                WriteStatus = GetElementValue(xml, "WriteStatus");
            }
            public string NrTracks { get; private set; }
            public string MediaDuration { get; private set; }
            public string CurrentURI { get; private set; }
            public string CurrentURIMetaData { get; private set; }
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
