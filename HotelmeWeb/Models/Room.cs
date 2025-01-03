using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


    namespace HotelmeWeb.Data
    {
        public class Room
        {
            public int RoomID { get; set; }
            public string RoomNumber { get; set; }
            public string Status { get; set; }
            public decimal PricePerNight { get; set; }
            public string RoomType { get; set; }
        }
    }

