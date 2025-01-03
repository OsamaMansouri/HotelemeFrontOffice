using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace HotelmeWeb.Data
{
    public class MySqlConnector
    {
        public string ConnectionString { get; private set; } = "Server=localhost;Database=hotelmanagementsystem;User=root;Password=;";

        public List<Room> GetRooms()
        {
            var rooms = new List<Room>();
            using (var connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                SELECT r.RoomID, r.RoomNumber, r.Status, r.PricePerNight, rt.TypeName AS RoomType 
                FROM rooms r
                JOIN roomtypes rt ON r.RoomTypeID = rt.RoomTypeID";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rooms.Add(new Room
                            {
                                RoomID = Convert.ToInt32(reader["RoomID"]),
                                RoomNumber = reader["RoomNumber"].ToString(),
                                Status = reader["Status"].ToString(),
                                PricePerNight = Convert.ToDecimal(reader["PricePerNight"]),
                                RoomType = reader["RoomType"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle errors
                }
            }
            return rooms;
        }


        public List<Client> GetClients()
        {
            var clients = new List<Client>();
            using (var connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM clients";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clients.Add(new Client
                            {
                                ClientID = Convert.ToInt32(reader["ClientID"]),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                Address = reader["Address"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle errors (optional logging)
                }
            }
            return clients;
        }

        public List<Reservation> GetReservations()
        {
            var reservations = new List<Reservation>();
            using (var connection = new MySqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        SELECT r.ReservationID, r.CheckInDate, r.CheckOutDate, r.NumberOfGuests, r.TotalPrice, c.Name AS ClientName, ro.RoomNumber
                        FROM reservations r
                        JOIN clients c ON r.ClientID = c.ClientID
                        JOIN rooms ro ON r.RoomID = ro.RoomID";
                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reservations.Add(new Reservation
                            {
                                ReservationID = Convert.ToInt32(reader["ReservationID"]),
                                CheckInDate = Convert.ToDateTime(reader["CheckInDate"]),
                                CheckOutDate = Convert.ToDateTime(reader["CheckOutDate"]),
                                NumberOfGuests = Convert.ToInt32(reader["NumberOfGuests"]),
                                TotalPrice = Convert.ToDecimal(reader["TotalPrice"]),
                                ClientName = reader["ClientName"].ToString(),
                                RoomNumber = reader["RoomNumber"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle errors (optional logging)
                }
            }
            return reservations;
        }
    }




    public class Client
    {
        public int ClientID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Reservation
    {
        public int ReservationID { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public string ClientName { get; set; }
        public string RoomNumber { get; set; }
    }
}
