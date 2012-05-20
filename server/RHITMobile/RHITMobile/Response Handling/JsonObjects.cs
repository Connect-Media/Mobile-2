﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Data;
using RHITMobile.RhitPrivate;

namespace RHITMobile {
    [DataContract]
    public abstract class JsonObject { }

    #region MessageResponse
    [DataContract]
    public class MessageResponse : JsonObject {
        public MessageResponse(string message, params object[] objects) {
            Message = String.Format(message, objects);
        }

        [DataMember]
        public string Message { get; set; }
    }

    [DataContract]
    public class VersionResponse : JsonObject {
        public VersionResponse() {
            LocationsVersion = Program.LocationsVersion;
            ServicesVersion = Program.ServicesVersion;
            TagsVersion = Program.TagsVersion;
        }

        [DataMember]
        public double LocationsVersion { get; set; }
        [DataMember]
        public double ServicesVersion { get; set; }
        [DataMember]
        public double TagsVersion { get; set; }
    }
    #endregion

    #region MapAreasResponse
    [DataContract]
    public class MapAreasResponse : JsonObject {
        public MapAreasResponse(double version) {
            Version = version;
            Areas = new List<MapArea>();
        }

        [DataMember]
        public List<MapArea> Areas { get; set; }
        [DataMember]
        public double Version { get; set; }
    }

    [DataContract]
    public class MapArea : JsonObject {
        public MapArea(DataRow row) {
            Id = (int)row["id"];
            Name = (string)row["name"];
            Description = (string)row["description"];
            LabelOnHybrid = (bool)row["labelonhybrid"];
            MinZoomLevel = (int)row["minzoomlevel"];
            Center = new LatLong(row);
            Corners = new List<LatLong>();
        }

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public LatLong Center { get; set; }
        [DataMember]
        public List<LatLong> Corners { get; set; }
        [DataMember]
        public bool LabelOnHybrid { get; set; }
        [DataMember]
        public int MinZoomLevel { get; set; }
    }
    #endregion

    #region LocationsResponse
    [DataContract]
    public class LocationsResponse : JsonObject {
        public LocationsResponse(double version) {
            Version = version;
            Locations = new List<Location>();
        }

        [DataMember]
        public List<Location> Locations { get; set; }
        [DataMember]
        public double Version { get; set; }
    }

    [DataContract]
    public class Location : JsonObject {
        public Location(DataRow row, bool hideDescs) {
            AltNames = new List<string>();
            Center = new LatLong(row);
            Description = hideDescs ? null : (string)row["description"];
            Floor = (int)row["floor"];
            Links = hideDescs ? null : new List<HyperLink>();
            Id = (int)row["id"];
            IsDepartable = !row.IsNull("departnode");
            MapArea = row.IsNull("labelonhybrid") ? null : new MapAreaData(row);
            Name = (string)row["name"];
            Parent = row.IsNull("parent") ? null : (int?)row["parent"];
            Type = (string)row["type"];

            HasAltNames = (bool)row["hasalts"];
            HasLinks = (bool)row["haslinks"];
        }

        [DataMember]
        public List<string> AltNames { get; set; }
        [DataMember]
        public LatLong Center { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int Floor { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public bool IsDepartable { get; set; }
        [DataMember]
        public List<HyperLink> Links { get; set; }
        [DataMember]
        public MapAreaData MapArea { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int? Parent { get; set; }
        [DataMember]
        public string Type { get; set; }

        public bool HasAltNames { get; set; }
        public bool HasLinks { get; set; }

        public bool IsMapArea() {
            return MapArea != null;
        }

        public void AddAltNames(DataTable table) {
            foreach (DataRow row in table.Rows) {
                AltNames.Add((string)row["name"]);
            }
        }

        public void AddLinks(DataTable table) {
            foreach (DataRow row in table.Rows) {
                Links.Add(new HyperLink(row));
            }
        }
    }

    [DataContract]
    public class MapAreaData : JsonObject {
        public MapAreaData(DataRow row) {
            Corners = new List<LatLong>();
            LabelOnHybrid = (bool)row["labelonhybrid"];
            MinZoomLevel = (int)row["minzoomlevel"];
        }

        [DataMember]
        public List<LatLong> Corners { get; set; }
        [DataMember]
        public bool LabelOnHybrid { get; set; }
        [DataMember]
        public int MinZoomLevel { get; set; }
    }

    [DataContract]
    public class HyperLinkShort : JsonObject {
        public HyperLinkShort(DataRow row) {
            Name = (string)row["name"];
            Url = (string)row["url"];
        }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Url { get; set; }
    }

    [DataContract]
    public class HyperLink : JsonObject {
        public HyperLink(DataRow row) {
            Name = (string)row["name"];
            Url = (string)row["url"];
            Type = (string)row["type"];
        }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string Type { get; set; }
    }
    #endregion

    #region LocationNamesResponse
    [DataContract]
    public class LocationNamesResponse : JsonObject {
        public LocationNamesResponse(double version) {
            Names = new List<LocationName>();
            Version = version;
        }

        [DataMember]
        public List<LocationName> Names { get; set; }
        [DataMember]
        public double Version { get; set; }
    }

    [DataContract]
    public class LocationName : JsonObject {
        public LocationName(DataRow row) {
            Id = (int)row["id"];
            Name = (string)row["name"];
        }

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
    }
    #endregion

    #region LocationDescResponse
    [DataContract]
    public class LocationDescResponse : JsonObject {
        public LocationDescResponse(DataRow row) {
            Description = (string)row["description"];
            Links = new List<HyperLink>();
        }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public List<HyperLink> Links { get; set; }
    }
    #endregion

    #region LocationIdResponse
    [DataContract]
    public class LocationIdResponse : JsonObject {
        public LocationIdResponse(int id) {
            Id = id;
        }

        [DataMember]
        public int Id { get; set; }
    }
    #endregion

    #region DirectionsResponse & PrinterResponse
    [DataContract]
    public class DirectionsResponse : JsonObject {
        public DirectionsResponse(int done, int requestId) {
            Done = done;
            RequestId = requestId;
            Result = null;
        }

        [DataMember]
        public int Done { get; set; }
        [DataMember]
        public int RequestId { get; set; }
        [DataMember]
        public Directions Result { get; set; }
    }

    [DataContract]
    public class PrinterResponse : DirectionsResponse {
        public PrinterResponse(int done, int requestId, string printer)
            : base(done, requestId) {
            Printer = printer;
        }

        [DataMember]
        public string Printer { get; set; }
    }

    [DataContract]
    public class Directions : JsonObject {
        public Directions(List<DirectionPath> paths) {
            //Dist = paths.Sum(path => path.HDist * path.HDist + path.VDist * path.VDist);
            Paths = paths;
            //StairsDown = -paths.Sum(path => Math.Min(path.Stairs, 0));
            //StairsUp = paths.Sum(path => Math.Max(path.Stairs, 0));
        }

        [DataMember]
        public double Dist { get; set; }
        [DataMember]
        public List<DirectionPath> Paths { get; set; }
        [DataMember]
        public int StairsDown { get; set; }
        [DataMember]
        public int StairsUp { get; set; }
    }

    [DataContract]
    public class DirectionPath : JsonObject {
        public DirectionPath(double lat, double lon, string dir, bool flag, string action, double alt, int? loc, bool outside, int id, bool forward)
        : this(lat, lon, dir, flag, action, alt, loc, outside) {
            Id = id;
            Forward = forward;
        }

        public DirectionPath(double lat, double lon, string dir, bool flag, string action, double alt, int? loc, bool outside) {
            Action = action;
            Altitude = alt;
            Dir = dir;
            Flag = flag;
            Lat = lat;
            Lon = lon;
            Location = loc;
            Outside = outside;
            Id = 0;
        }

        [DataMember]
        public string Action { get; set; }
        [DataMember]
        public double Altitude { get; set; }
        [DataMember]
        public string Dir { get; set; }
        [DataMember]
        public bool Flag { get; set; }
        [DataMember]
        public double Lat { get; set; }
        [DataMember]
        public int? Location { get; set; }
        [DataMember]
        public double Lon { get; set; }
        [DataMember]
        public bool Outside { get; set; }

        public int Id { get; set; }
        public bool Forward { get; set; }
    }
    #endregion

    #region TagsResponse & LocationIdsResponse
    [DataContract]
    public class TagsResponse : JsonObject {
        public TagsResponse(double version) {
            Version = version;
            TagsRoot = new TagCategory();
        }

        [DataMember]
        public TagCategory TagsRoot { get; set; }
        [DataMember]
        public double Version { get; set; }
    }

    [DataContract]
    public class TagCategory : JsonObject {
        public TagCategory(DataRow row) : this() {
            Name = (string)row["name"];

            if (!row.IsNull("parent"))
                Parent = (string)row["parent"];
        }

        public TagCategory() {
            Children = new List<TagCategory>();
            Tags = new List<Tag>();
            Name = "";
            Parent = null;
        }

        [DataMember]
        public List<TagCategory> Children { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<Tag> Tags { get; set; }

        public string Parent { get; set; }
        public bool IsDefault { get; set; }
    }

    [DataContract]
    public class Tag : JsonObject {
        public Tag(DataRow row) {
            Id = (int)row["id"];
            Name = (string)row["name"];
            IsDefault = (bool)row["isdefault"];
        }

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public bool IsDefault { get; set; }
        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    public class LocationIdsResponse : JsonObject {
        public LocationIdsResponse(List<int> locations) {
            LocationIds = locations;
        }

        [DataMember]
        public List<int> LocationIds { get; set; }
    }
    #endregion

    #region CampusServicesResponse
    [DataContract]
    public class CampusServicesResponse : JsonObject {
        public CampusServicesResponse(double version) {
            Version = version;
            ServicesRoot = new CampusServicesCategory();
        }

        [DataMember]
        public CampusServicesCategory ServicesRoot { get; set; }
        [DataMember]
        public double Version { get; set; }
    }

    [DataContract]
    public class CampusServicesCategory : JsonObject {
        public CampusServicesCategory(DataRow row) : this() {
            Name = (string)row["name"];
            if (!row.IsNull("parent"))
                Parent = (string)row["parent"];
        }

        public CampusServicesCategory() {
            Children = new List<CampusServicesCategory>();
            Links = new List<HyperLinkShort>();
            Name = "";
            Parent = null;
        }

        [DataMember]
        public List<CampusServicesCategory> Children { get; set; }
        [DataMember]
        public List<HyperLinkShort> Links { get; set; }
        [DataMember]
        public string Name { get; set; }

        public string Parent { get; set; }
    }
    #endregion

    #region Admin Interface
    [DataContract]
    public class SAdminAuthResponse : JsonObject {
        public SAdminAuthResponse(DateTime expiration, Guid id) {
            Expiration = expiration;
            Token = id.ToString();
        }

        public SAdminAuthResponse(DateTime expiration, string token) {
            Expiration = expiration;
            Token = token;
        }

        [DataMember]
        public DateTime Expiration { get; set; }
        [DataMember]
        public string Token { get; set; }
    }

    [DataContract]
    public class StoredProcedureResponse : JsonObject {
        public StoredProcedureResponse() {
            Columns = new List<string>();
            Table = new List<List<string>>();
        }

        [DataMember]
        public List<string> Columns { get; set; }
        [DataMember]
        public List<List<string>> Table { get; set; }
    }

    [DataContract]
    public class PathDataResponse : JsonObject {
        public PathDataResponse(double version) {
            Directions = new List<Direction>();
            Messages = new List<DirectionMessage>();
            Nodes = new List<Node>();
            Partitions = new List<Partition>();
            Paths = new List<Path>();
            Version = version;
        }

        [DataMember]
        public List<Direction> Directions { get; set; }
        [DataMember]
        public List<DirectionMessage> Messages { get; set; }
        [DataMember]
        public List<Node> Nodes { get; set; }
        [DataMember]
        public List<Partition> Partitions { get; set; }
        [DataMember]
        public List<Path> Paths { get; set; }
        [DataMember]
        public double Version { get; set; }
    }

    [DataContract]
    public class Path : JsonObject {
        public Path(DataRow row) : this(row, (int)row["node1"], (int)row["node2"], (int)row["partition"]) { }

        public Path(DataRow row, int node1, int node2, int partition) {
            Elevator = (bool)row["elevator"];
            if (row.Table.Columns.Contains("pathid"))
                Id = (int)row["pathid"];
            else
                Id = (int)row["id"];
            Stairs = (int)row["stairs"];
            Node1 = node1;
            Node2 = node2;
            Partition = partition;
        }

        [DataMember]
        public bool Elevator { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int Node1 { get; set; }
        [DataMember]
        public int Node2 { get; set; }
        [DataMember]
        public int Partition { get; set; }
        [DataMember]
        public int Stairs { get; set; }
    }

    [DataContract]
    public class Node : JsonObject {
        public Node(DataRow row) {
            Id = (int)row["id"];
            Lat = (double)row["lat"];
            Lon = (double)row["lon"];
            Altitude = (double)row["altitude"];
            Outside = (bool)row["outside"];
            Location = row.IsNull("location") ? null : (int?)row["location"];
            Partition = row.Table.Columns.Contains("partition") ? (int?)row["partition"] : null;
            Floor = row.Table.Columns.Contains("floor") ? (row.IsNull("floor") ? null : (int?)row["floor"]) : null;
        }

        public Node(DirectionPath path) {
            Id = path.Id;
            Lat = path.Lat;
            Lon = path.Lon;
            Altitude = path.Altitude;
            Outside = path.Outside;
            Location = path.Location;
            Partition = null;
            Floor = null;
        }

        [DataMember]
        public double Altitude { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public double Lat { get; set; }
        [DataMember]
        public int? Location { get; set; }
        [DataMember]
        public double Lon { get; set; }
        [DataMember]
        public bool Outside { get; set; }
        [DataMember]
        public int? Floor { get; set; }
        
        public int? Partition { get; set; }

        public double HDistanceTo(Node node) {
            return HDistanceTo(node.Lat, node.Lon);
        }

        public double HDistanceTo(double lat, double lon) {
            var x = (lon - this.Lon) * Program.DegToRad * Math.Cos((lat + this.Lat) * Program.DegToRad / 2);
            var y = (lat - this.Lat) * Program.DegToRad;
            return Math.Sqrt(x * x + y * y) * Program.EarthRadius;
        }
    }

    [DataContract]
    public class Direction : JsonObject {
        public Direction(DataRow row) {
            Id = (int)row["id"];
            Message = (int)row["message"];
            Paths = new List<int>() { (int)row["startpath"] };
            Within = row.IsNull("within") ? null : (int?)row["within"];
        }

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int Message { get; set; }
        [DataMember]
        public List<int> Paths { get; set; }
        [DataMember]
        public int? Within { get; set; }
    }

    [DataContract]
    public class Partition : JsonObject {
        public Partition(DataRow row) {
            Id = (int)row["id"];
            Description = (string)row["description"];
        }

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Description { get; set; }
    }

    [DataContract]
    public class DirectionMessage : JsonObject {
        public DirectionMessage(DataRow row) {
            Id = (int)row["id"];
            Message1 = row.IsNull("message1") ? null : (string)row["message1"];
            Message2 = row.IsNull("message2") ? null : (string)row["message2"];
            Action1 = row.IsNull("action1") ? null : (string)row["action1"];
            Action2 = row.IsNull("action2") ? null : (string)row["action2"];
            NodeOffset = (int)row["nodeoffset"];
        }

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Message1 { get; set; }
        [DataMember]
        public string Message2 { get; set; }
        [DataMember]
        public string Action1 { get; set; }
        [DataMember]
        public string Action2 { get; set; }
        [DataMember]
        public int NodeOffset { get; set; }
    }
    #endregion

    #region UserDataResponse, UsersResponse, CoursesResponse, & BannerAuthResponse
    [DataContract]
    public class BannerAuthResponse : JsonObject {
        public BannerAuthResponse(DateTime expiration, Guid id, int term, List<Term> terms) {
            Expiration = expiration;
            Token = id.ToString();
            CurrentTerm = term;
            Terms = terms;
        }

        public BannerAuthResponse(DateTime expiration, string token, int term, List<Term> terms) {
            Expiration = expiration;
            Token = token;
            CurrentTerm = term;
            Terms = terms;
        }

        [DataMember]
        public DateTime Expiration { get; set; }
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public int CurrentTerm { get; set; }
        [DataMember]
        public List<Term> Terms { get; set; }
    }

    [DataContract]
    public class UserDataResponse : JsonObject {
        public UserDataResponse(User user) {
            Advisor = null;
            Class = user.Class;
            CM = user.Mailbox;
            Department = user.Department;
            Email = user.Alias;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Majors = user.Major;
            MiddleName = user.MiddleName;
            Office = user.Room;
            Telephone = user.Phone;
            User = new ShortUser(user);
            Year = user.Year;
        }

        public UserDataResponse(User user, User advisor)
            : this(user) {
            Advisor = new ShortUser(advisor);
        }

        [DataMember]
        public ShortUser Advisor { get; set; }
        [DataMember]
        public string Class { get; set; }
        [DataMember]
        public int CM { get; set; }
        [DataMember]
        public string Department { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Majors { get; set; }
        [DataMember]
        public string MiddleName { get; set; }
        [DataMember]
        public string Office { get; set; }
        [DataMember]
        public string Telephone { get; set; }
        [DataMember]
        public ShortUser User { get; set; }
        [DataMember]
        public string Year { get; set; }
    }

    [DataContract]
    public class UsersResponse : JsonObject {
        public UsersResponse(User[] users) {
            Users = users.Select(user => new ShortUser(user)).ToList();
        }

        [DataMember]
        public List<ShortUser> Users { get; set; }
    }

    [DataContract]
    public class CoursesResponse : JsonObject {
        public CoursesResponse() {
            Courses = new List<SCourse>();
        }

        [DataMember]
        public List<SCourse> Courses { get; set; }
    }

    [DataContract]
    public class SCourse : JsonObject {
        public SCourse(Course course) {
            Comments = course.Comments;
            CourseNumber = course.Name;
            Credits = course.Credit;
            CRN = course.CRN;
            Enrolled = course.Enrolled;
            FinalDay = course.FinalDay + "";
            FinalHour = course.FinalHour;
            FinalRoom = course.FinalRoom;
            MaxEnrolled = course.MaxEnrollment;
            Term = course.Term;
            Title = course.Title;
        }

        [DataMember]
        public string Comments { get; set; }
        [DataMember]
        public string CourseNumber { get; set; }
        [DataMember]
        public int Credits { get; set; }
        [DataMember]
        public int CRN { get; set; }
        [DataMember]
        public int Enrolled { get; set; }
        [DataMember]
        public string FinalDay { get; set; }
        [DataMember]
        public int? FinalHour { get; set; }
        [DataMember]
        public string FinalRoom { get; set; }
        [DataMember]
        public ShortUser Instructor { get; set; }
        [DataMember]
        public int MaxEnrolled { get; set; }
        [DataMember]
        public List<CourseMeeting> Schedule { get; set; }
        [DataMember]
        public List<ShortUser> Students { get; set; }
        [DataMember]
        public int Term { get; set; }
        [DataMember]
        public string Title { get; set; }
    }

    [DataContract]
    public class CourseMeeting : JsonObject {
        public CourseMeeting(CourseTime time) {
            Day = time.Day + "";
            EndPeriod = time.EndPeriod;
            StartPeriod = time.StartPeriod;
            Room = time.Room;
        }

        public CourseMeeting(RoomSchedule time, string room) {
            Day = time.Day + "";
            EndPeriod = time.EndPeriod;
            StartPeriod = time.StartPeriod;
            Room = room;
        }

        [DataMember]
        public string Day { get; set; }
        [DataMember]
        public int EndPeriod { get; set; }
        [DataMember]
        public int StartPeriod { get; set; }
        [DataMember]
        public string Room { get; set; }
    }

    [DataContract]
    public class ShortUser : JsonObject {
        public ShortUser(User user) {
            Username = user.Username;

            if (String.IsNullOrWhiteSpace(user.MiddleName))
                FullName = String.Format("{0} {1}", user.FirstName, user.LastName);
            else
                FullName = String.Format("{0} {1} {2}", user.FirstName, user.MiddleName, user.LastName);

            int isStudent = 0;
            isStudent += String.IsNullOrWhiteSpace(user.Advisor) ? -1 : 1;
            isStudent += String.IsNullOrWhiteSpace(user.Class) ? -1 : 1;
            isStudent += String.IsNullOrWhiteSpace(user.Department) ? 1 : -1;
            isStudent += String.IsNullOrWhiteSpace(user.Major) ? -1 : 1;
            isStudent += String.IsNullOrWhiteSpace(user.Year) ? -1 : 1;

            if (isStudent > 0) {
                if (!String.IsNullOrWhiteSpace(user.Class) && !String.IsNullOrWhiteSpace(user.Major))
                    Subtitle = String.Format("{0} {1} Student", user.Class, user.Major);
                else if (!String.IsNullOrWhiteSpace(user.Class))
                    Subtitle = String.Format("{0} Student", user.Class);
                else if (!String.IsNullOrWhiteSpace(user.Major))
                    Subtitle = String.Format("{0} Student", user.Major);
                else
                    Subtitle = "Student";
            } else {
                if (!String.IsNullOrWhiteSpace(user.Department))
                    Subtitle = String.Format("Faculty in {0}", user.Department);
                else
                    Subtitle = "Faculty";
            }
        }

        [DataMember]
        public string FullName { get; set; }
        [DataMember]
        public string Subtitle { get; set; }
        [DataMember]
        public string Username { get; set; }
    }

    [DataContract]
    public class Term {
        public Term(int id) {
            Id = id;
            string quarter;
            switch (id % 100) {
                case 10: quarter = "Fall"; break;
                case 20: quarter = "Winter"; break;
                case 30: quarter = "Spring"; break;
                case 40: quarter = "Summer"; break;
                default: quarter = "Unknown"; break;
            }
            Name = String.Format("{0} Quarter - {1:0000}-{2:00}", quarter, id / 100 - 1, (id / 100) % 100);
        }

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
    }
    #endregion

    #region FoldersResponse
    [DataContract]
    public class FoldersResponse : JsonObject {
        public FoldersResponse(List<string> folders) {
            Folders = folders;
        }

        [DataMember]
        public List<string> Folders { get; set; }
    }
    #endregion

    #region Miscellaneous
    [DataContract]
    public class LatLong : JsonObject {
        public LatLong(DataRow row) {
            Lat = (double)row["lat"];
            Lon = (double)row["lon"];
        }

        public LatLong(double lat, double lon) {
            Lat = lat;
            Lon = lon;
        }

        [DataMember]
        public double Lat { get; set; }
        [DataMember]
        public double Lon { get; set; }
    }
    #endregion

    public static class JsonUtility {
        public static T Deserialize<T>(this string json)
            where T : JsonObject {
            MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            return (T)serializer.ReadObject(stream);
        }

        public static string Serialize<T>(this T obj)
            where T : JsonObject {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            serializer.WriteObject(stream, obj);
            stream.Position = 0;
            using (var reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }
    }
}
