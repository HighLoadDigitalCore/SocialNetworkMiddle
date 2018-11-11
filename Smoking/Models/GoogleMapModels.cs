using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Linq;
using System.Data.Linq.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using Smoking.Extensions;
using Smoking.Extensions.Helpers;

namespace Smoking.Models
{


    [Serializable]
    public class MapFilterData : JsonTransport
    {
        public static string[] ParamNames = new[] { "SearchWord", "SmokingType", "IsFavorite" };
        public static string[] JsonParamNames = new[] { "ObjectFilter", "CurrentMapView", "NewPointData" };
        public static string[] JsonClassNames = new[] { "Smoking.Models.ObjectFilter", "Smoking.Models.MapViewPort", "Smoking.Models.PointData" };
        public static string[] ParamNamesToOut = new[] { "SearchWord", "ObjectFilter", "SmokingType", "CurrentMapView", "IsFavorite" };

        public MapFilterData()
        {

        }
        public MapFilterData(NameValueCollection collection)
        {
            foreach (var field in ParamNames)
            {
                this.SetPropertyValue(field,
                                      collection[field].IsNullInt()
                                          ? collection[field].ToNullInt()
                                          : (object)collection[field]);
            }
            var counter = 0;
            foreach (var j in JsonParamNames)
            {
                Type type = Type.GetType(JsonClassNames[counter], true);
                object instance = Activator.CreateInstance(type);
                MethodInfo theMethod = instance.GetType().GetMethod("FromJson");
                MethodInfo generic = theMethod.MakeGenericMethod(instance.GetType());
                var obj = generic.Invoke(instance, new object[] { collection[j] });
                this.SetPropertyValue(j, obj);
                counter++;
            }
        }

        public MapFilterData(FormCollection collection)
            : this(collection as NameValueCollection)
        {
        }

        public ObjectFilter ObjectFilter { get; set; }
        public string SearchWord { get; set; }
        public int? SmokingType { get; set; }
        public int? IsFavorite { get; set; }
        public MapViewPort CurrentMapView { get; set; }
        public int[] LoadedPoints { get; set; }

        [ScriptIgnore]
        public override object DefVal
        {
            get { return new MapFilterData(); }
        }

        public List<UploadingPointData> GetArrayForUpload()
        {

            var user = Membership.GetUser();
            Guid myUID = user == null ? new Guid() : (Guid)user.ProviderUserKey;
            var db = new DB();
            IQueryable<MapObject> objList = db.MapObjects.AsQueryable();

            if (SearchWord.IsFilled())
            {
                objList = objList.Where(
                    x =>
                    SqlMethods.Like(x.Address ?? "", "%" + SearchWord.Replace(" ", "%") + "%") ||
                    SqlMethods.Like(x.Name ?? "", "%" + SearchWord.Replace(" ", "%") + "%") ||
                    SqlMethods.Like(x.Description ?? "", "%" + SearchWord.Replace(" ", "%") + "%"));
            }
            else
            {
                objList =
                    objList.Where(
                        x =>
                        x.MapCoords.Any(
                            z =>
                            z.XPos >= CurrentMapView.LeftUpperCorner.Lat &&
                            z.XPos <= CurrentMapView.RightBottomCorner.Lat &&
                            z.YPos >= CurrentMapView.RightBottomCorner.Lng &&
                            z.YPos <= CurrentMapView.LeftUpperCorner.Lng));
            }
            if (IsFavorite == 1 && user != null)
            {
                objList = objList.Where(x => x.CreatorID == myUID);
            }

            if (SmokingType.HasValue)
            {
                objList = objList.Where(x => x.ObjectType == SmokingType);
            }

            if (ObjectFilter.SelectedTypes.Any())
            {
                var typeFiltered = (from type in ObjectFilter.SelectedTypes
                                    let qs = objList.Where(x => x.TypeID == type.TypeID)
                                    select
                                        type.ObjectType == 0
                                            ? qs.Where(x => x.MapCoords.All(z => z.IsMarker))
                                            : qs.Where(x => x.MapCoords.Any(z => !z.IsMarker)))
                    .Aggregate<IQueryable<MapObject>, IQueryable<MapObject>>(null,
                                                                             (current, qs) =>
                                                                             current == null ? qs : current.Concat(qs));
                objList = typeFiltered;
            }


            if (LoadedPoints.Any())
            {
                objList = objList.Where(x => !LoadedPoints.Take(500).Contains(x.ID));
            }
            return objList.Take(100).ToList().Select(x => x.ToUploadData()).ToList();
        }
    }

    public partial class MapCoord
    {
        public Coordinate ToCoordinate()
        {
            return new Coordinate() { Lat = XPos, Lng = YPos };
        }
    }

    public partial class MapObject
    {


        public static MapObject getByID(int? id = null)
        {
            if (!id.HasValue)
                id = HttpContext.Current.Request["oid"].ToNullInt();
            if (!id.HasValue)
                return null;
            return new DB().MapObjects.FirstOrDefault(x => x.ID == id);
        }

        public string SmokingStatus
        {
            get
            {
                if (ObjectType == -1)
                    return "Не курят";
                if (ObjectType == 0)
                    return "Спорная";
                if (ObjectType == 1)
                    return "Курят";

                return "";
            }
        }

        public bool CanEdit
        {
            get { return AccessHelper.IsMaster || CreatorID == AccessHelper.CurrentUserKey; }
        }

        public string SmokingStatusShort
        {
            get
            {
                if (ObjectType == -1)
                    return "Здесь не курят";
                if (ObjectType == 0)
                    return "Спорная";
                if (ObjectType == 1)
                    return "Здесь курят";

                return "";
            }
        }


        public string CommentPageLink
        {
            get
            {
                var pages = CMSPage.GetByType("ProfileZones").ToList();
                return !pages.Any() ? "#" : (pages.First().FullUrl + "?oid=" + this.ID + "&uid=" + CreatorID);
            }
        }
        public string PhotoLink
        {
            get
            {
                if (MapObjectPhoto != null)
                    return UniversalEditorPagedData.GetImageWrapper("MapObjectPhotos", "ObjectID", ID.ToString(), "RawData");
                return "/content/client/i/noimages.jpg";
            }
        }

        public UploadingPointData ToUploadData()
        {
            var user = Membership.GetUser();
            var myUID = user == null ? new Guid() : (Guid)user.ProviderUserKey;
            return new UploadingPointData()
                {
                    ID = ID,
                    IsMyPoint = CreatorID == myUID || AccessHelper.IsMaster,
                    IsRegion = MapCoords.Any(z => !z.IsMarker),
                    PointPosition = MapCoords.First(z => z.IsMarker).ToCoordinate(),
                    Description = Description,
                    Name = Name,
                    HeaderText =
                        Address + " / " +
                        (ObjectType == -1
                             ? " Зона «Не курят» / Курение запрещено"
                             : (ObjectType == 0
                                    ? " Зона «Спорная» / Курение разрешено"
                                    : " Зона «Курят» / Курение разрешено")),
                    CommentsLink = CMSPage.Get("myobjects").FullUrl + "?oid=" + ID + "&uid=" + CreatorID,
                    EditLink = CMSPage.Get("map").FullUrl + "#EditObj=" + ID,
                    ImageLink =
                        MapObjectPhoto == null
                            ? ""
                            : UniversalEditorPagedData.GetImageWrapper("MapObjectPhotos", "ObjectID", ID.ToString(),
                                                                       "RawData"),
                    IsMyFavorite = false, //CreatorID == myUID,
                    Address = Address,
                    TypeID = TypeID,
                    CommentCount = MapObjectComments.Count,
                    SmokingType = ObjectType,
                    RegionPosition =
                        MapCoords.Where(z => !z.IsMarker)
                                 .OrderBy(x => x.OrderNum)
                                 .ToList()
                                 .Select(x => x.ToCoordinate())
                                 .ToList(),
                    IconNum = MapObjectType.Icon.Replace("icon-obj", ""),
                    UserName = User.UserProfile.FullName,
                    UserLink = User.UserProfile.EditProfilePage
                };
        }
    }

    [Serializable]
    public class UploadingPointData
    {
        public int ID { get; set; }
        public bool IsMyPoint { get; set; }
        public bool IsRegion { get; set; }
        public Coordinate PointPosition { get; set; }
        public List<Coordinate> RegionPosition { get; set; }
        public bool IsMyFavorite { get; set; }
        public string Address { get; set; }
        public string HeaderText { get; set; }
        public string Name { get; set; }
        public string ImageLink { get; set; }
        public string CommentsLink { get; set; }
        public string EditLink { get; set; }
        public int CommentCount { get; set; }
        public string Description { get; set; }
        public int SmokingType { get; set; }
        public string IconNum { get; set; }
        public int TypeID { get; set; }
        public string UserName { get; set; }
        public string UserLink { get; set; }

    }


    [Serializable]
    public class PointData : JsonTransport
    {

        
        public Coordinate[] Polygon { get; set; }
        public bool HasPolygon { get; set; }
        public Coordinate Marker { get; set; }
        public FormData UserData { get; set; }

        public override object DefVal
        {
            get { return new PointData() { HasPolygon = false, Polygon = null, UserData = new FormData() }; }
        }

        public MapObject ToDBModel()
        {
            var mo = new MapObject()
                {
                    Address = UserData.ZoneAdress,
                    ObjectType = UserData.ZoneSmokingType,
                    TypeID = UserData.ZoneType,
                    CreateDate = DateTime.Now,
                    CreatorID = (Guid)Membership.GetUser().ProviderUserKey,
                    Name = UserData.ZoneName,
                    Description = UserData.ZoneDescr,
                };

            if (HasPolygon)
            {
                var coords =
                    Polygon.Select(
                        (x, index) =>
                        new MapCoord() { IsMarker = false, XPos = x.Lat, YPos = x.Lng, OrderNum = index, MapObject = mo }).ToList();
            }

            var mainCoord = new MapCoord()
                {
                    IsMarker = true,
                    XPos = Marker.Lat,
                    YPos = Marker.Lng,
                    OrderNum = 0,
                    MapObject = mo
                };

            if (UserData.ZonePhoto.IsFilled())
            {
                var fi = new FileInfo(HttpContext.Current.Server.MapPath(UserData.ZonePhoto));
                var photo = new MapObjectPhoto() { MapObject = mo, RawData = fi.ToBinary() };
            }

            return mo;
        }
    }

    [Serializable]
    public class FormData
    {
        public int ID { get; set; }
        public string ZoneAdress { get; set; }
        public string ZoneName { get; set; }
        public int ZoneType { get; set; }
        public int ZoneSmokingType { get; set; }
        public string ZoneDescr { get; set; }
        public string ZonePhoto { get; set; }
    }


    [Serializable]
    public class ObjectFilter : JsonTransport
    {
        /// <summary>
        /// Выбранные в фильтре типы объектов
        /// </summary>
        public List<ObjectInFilter> SelectedTypes { get; set; }




        //Свойства для вывода
        public List<ObjectTypeItem> ObjectTypeList { get; set; }
        public int ActiveList { get; set; }
        [ScriptIgnore]
        public override object DefVal
        {
            get
            {
                return new ObjectFilter()
                    {
                        SelectedTypes = new List<ObjectInFilter>(),
                        ActiveList = 0,
                        ObjectTypeList =
                            new DB().MapObjectTypes.Select(x => new ObjectTypeItem() { ID = x.ID, Name = x.TypeName })
                                    .ToList()
                    };
            }
        }
    }

    [Serializable]
    public class ObjectTypeItem
    {
        public string Name { get; set; }
        public int ID { get; set; }
    }

    [Serializable]
    public class ObjectInFilter
    {
        public ObjectInFilter()
        {
            TypeID = ObjectType = 0;
        }

        public int TypeID { get; set; }
        public int ObjectType { get; set; }
    }

    [Serializable]
    public class MapViewPort : JsonTransport
    {
        public Coordinate MapCenter { get; set; }
        public Coordinate LeftUpperCorner { get; set; }
        public Coordinate RightBottomCorner { get; set; }
        public int Zoom { get; private set; }

        [ScriptIgnore]
        public override object DefVal
        {
            get
            {
                return new MapViewPort()
                    {
                        //https://www.google.ru/maps/preview#!data=!1m4!1m3!1d252563!2d37.5827574!3d55.7583574
                        MapCenter = new Coordinate() { Lat = (decimal)55.7583574, Lng = (decimal)37.5827574 },
                        Zoom = 8,
                        LeftUpperCorner = MapCenter,
                        RightBottomCorner = MapCenter,
                    };
            }
        }
    }


    [Serializable]
    public abstract class JsonTransport
    {
        [ScriptIgnore]
        public abstract object DefVal { get; }

        public virtual string ToJson()
        {
            var serilizer = new JavaScriptSerializer();
            return serilizer.Serialize(this);
        }

        public T FromJson<T>(string json)
        {

            var serilizer = new JavaScriptSerializer();
            try
            {
                var obj = serilizer.Deserialize(json, typeof(T)) ?? (T)this.GetPropertyValue("DefVal");
                return (T)obj;
            }
            catch (Exception e)
            {
                return (T)this.GetPropertyValue("DefVal");
            }
        }
    }

    [Serializable]
    public class Coordinate
    {
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
    }
}