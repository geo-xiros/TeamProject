using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using TeamProject.Dal;
using TeamProject.Managers;
using TeamProject.Models;

namespace TeamProject.Managers
{
    public class BookingManager : TableManager<Booking>
    {

        public BookingManager(ProjectDbContext projectDbContext)            
        {
            AddField(b => b.CourtId);
            AddField(b => b.UserId);
            AddField(b => b.BookedAt);
            AddField(b => b.Duration);
            AddField(b => b.BookKey);
            PrepareQueries();

            _db = projectDbContext;
        }

        public override IEnumerable<Booking> Get(string queryWhere = null, object parameters = null)
        {
            IEnumerable<Booking> Bookings = Enumerable.Empty<Booking>();

            

            _db.UsingConnection((dbCon) =>
            {
                Bookings = dbCon.Query<Booking, Court, User, Booking>(
                "SELECT * FROM Booking " +
                "INNER JOIN Court ON Booking.CourtId = Court.Id " +
                "INNER JOIN [User] ON Booking.UserId = [User].Id" +
                (queryWhere == null ? string.Empty : $" WHERE {queryWhere}"),
                (Booking, court, user) =>
                {

                    Booking.Court = court;
                    Booking.User = user;
                    return Booking;

                },

            splitOn: "id",
                    param: parameters)
                    .Distinct()
                    .ToList();
            });

            return Bookings;
        }
        public override Booking Add(Booking book)
        {

            _db.UsingConnection(dbCon =>
            {
                book.Id = dbCon.ExecuteScalar<int>(
                    "IF (SELECT Count(*) FROM Booking WHERE CourtId = @CourtId AND BookedAt = @BookedAt)=0 " +
                    "BEGIN" +
                    "    INSERT INTO Booking (CourtId, BookedAt, UserId, Duration, BookKey) " +
                    "    VALUES (@CourtId, @BookedAt, @UserId, @Duration, @BookKey) " +
                    "    SELECT SCOPE_IDENTITY() AS Id " +
                    "END",
                    book);
            });
            return book;
        }

    }
}
