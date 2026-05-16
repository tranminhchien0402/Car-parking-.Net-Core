using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository;
using Repository.Models;

namespace Service
{
    public class ParkingLotService
    {
        private readonly ParkingLotRepository parkingLotRepo = new ParkingLotRepository();

        public (int total, int occupied, int free) GetParkingStatus()
            => parkingLotRepo.GetParkingStatus();

        public decimal GetRevenueByDate(DateTime date)
            => parkingLotRepo.GetRevenueByDate(date);

        public decimal GetRevenueByMonth(int year, int month)
            => parkingLotRepo.GetRevenueByMonth(year, month);

        public decimal GetRevenueByYear(int year)
            => parkingLotRepo.GetRevenueByYear(year);
    }
}
