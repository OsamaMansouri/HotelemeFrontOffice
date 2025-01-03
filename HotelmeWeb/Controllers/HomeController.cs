using System;
using System.Linq;
using System.Web.Mvc;
using HotelmeWeb.Data;
using MySql.Data.MySqlClient;

namespace HotelmeWeb.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Rooms()
        {
            var dbConnector = new MySqlConnector();
            var rooms = dbConnector.GetRooms();
            return View(rooms);
        }

        public ActionResult Reserve(int id)
        {
            var dbConnector = new MySqlConnector();
            var room = dbConnector.GetRooms().FirstOrDefault(r => r.RoomID == id);
            return View(room);
        }
        [HttpPost]
        public ActionResult Reserve(int roomID, string clientName, string clientEmail, string clientPhone, string clientAddress, DateTime checkInDate, DateTime checkOutDate, int numberOfGuests)
        {
            var dbConnector = new MySqlConnector();
            int clientID = 0;

            using (var connection = new MySqlConnection(dbConnector.ConnectionString))
            {
                try
                {
                    connection.Open();

                    // Insert client into the `clients` table
                    string insertClientQuery = @"
                INSERT INTO clients (Name, Email, Phone, Address, CreatedAt)
                VALUES (@Name, @Email, @Phone, @Address, NOW());
                SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(insertClientQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Name", clientName);
                        command.Parameters.AddWithValue("@Email", clientEmail);
                        command.Parameters.AddWithValue("@Phone", clientPhone);
                        command.Parameters.AddWithValue("@Address", clientAddress);
                        clientID = Convert.ToInt32(command.ExecuteScalar());
                    }

                    // Calculate total price based on the room's price and number of nights
                    string getRoomPriceQuery = "SELECT PricePerNight FROM rooms WHERE RoomID = @RoomID";
                    decimal pricePerNight = 0;

                    using (var command = new MySqlCommand(getRoomPriceQuery, connection))
                    {
                        command.Parameters.AddWithValue("@RoomID", roomID);
                        pricePerNight = Convert.ToDecimal(command.ExecuteScalar());
                    }

                    int totalNights = (checkOutDate - checkInDate).Days;
                    decimal totalPrice = totalNights * pricePerNight * numberOfGuests;

                    // Insert reservation into the `reservations` table
                    string insertReservationQuery = @"
                INSERT INTO reservations (RoomID, ClientID, CheckInDate, CheckOutDate, NumberOfGuests, TotalPrice, Status)
                VALUES (@RoomID, @ClientID, @CheckInDate, @CheckOutDate, @NumberOfGuests, @TotalPrice, 'Pending')";

                    using (var command = new MySqlCommand(insertReservationQuery, connection))
                    {
                        command.Parameters.AddWithValue("@RoomID", roomID);
                        command.Parameters.AddWithValue("@ClientID", clientID);
                        command.Parameters.AddWithValue("@CheckInDate", checkInDate);
                        command.Parameters.AddWithValue("@CheckOutDate", checkOutDate);
                        command.Parameters.AddWithValue("@NumberOfGuests", numberOfGuests);
                        command.Parameters.AddWithValue("@TotalPrice", totalPrice);



                        command.ExecuteNonQuery();
                        SendReservationEmail(clientName, clientEmail, checkInDate, checkOutDate, totalPrice);


                    }
                }
                catch (Exception ex)
                {
                    // Handle errors
                }

                return RedirectToAction("Confirmation");

            }

            return RedirectToAction("Rooms");
        }



        public ActionResult Confirmation()
        {
            return View();
        }


        private void SendReservationEmail(string clientName, string clientEmail, DateTime checkInDate, DateTime checkOutDate, decimal totalPrice)
        {
            try
            {
                string emailBody = $@"
        <html>
        <body>
            <h2>Reservation Confirmation</h2>
            <p>Dear {clientName},</p>
            <p>Thank you for choosing our hotel. Your reservation details are as follows:</p>
            <table style='width:100%; border:1px solid #ddd; border-collapse: collapse;'>
                <tr style='background-color: #f2f2f2;'>
                    <th style='border: 1px solid #ddd; padding: 8px;'>Check-In Date</th>
                    <th style='border: 1px solid #ddd; padding: 8px;'>Check-Out Date</th>
                    <th style='border: 1px solid #ddd; padding: 8px;'>Total Price</th>
                </tr>
                <tr>
                    <td style='border: 1px solid #ddd; padding: 8px;'>{checkInDate.ToShortDateString()}</td>
                    <td style='border: 1px solid #ddd; padding: 8px;'>{checkOutDate.ToShortDateString()}</td>
                    <td style='border: 1px solid #ddd; padding: 8px;'>${totalPrice:F2}</td>
                </tr>
            </table>
            <p>We look forward to hosting you!</p>
        </body>
        </html>";

                using (var smtpClient = new System.Net.Mail.SmtpClient("sandbox.smtp.mailtrap.io", 2525))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential("7c3c4df3537622", "6e811454b719f0");
                    smtpClient.EnableSsl = true;

                    var mailMessage = new System.Net.Mail.MailMessage
                    {
                        From = new System.Net.Mail.MailAddress("contact@hotelme.com", "Hotelme"),
                        Subject = "Reservation Confirmation",
                        Body = emailBody,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(clientEmail);
                    smtpClient.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
            }
        }



    }
}
