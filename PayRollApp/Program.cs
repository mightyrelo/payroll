using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PayRollApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //var members = new List<Staff>();
            var fileReader = new FileReader();
            int year = 0, month = 0;
            while(year == 0)
            {
                Console.WriteLine("Enter year: ");
                try {
                    year = Convert.ToInt32(Console.ReadLine());
                } catch (FormatException e) { 
                }
            }
            while(month == 0)
            {
                Console.WriteLine("Enter month: ");
                try {
                    month = Convert.ToInt32(Console.ReadLine());
                } catch (FormatException e) { }
            }
            //read from file, write to program memory
            var members = fileReader.ReadFile();
            //for all members, get hours worked and calculate pay
            for(int m = 0; m < members.Count; m++) {
                Console.WriteLine("{0}, hours worked: ", members[m].NameOfStaff);
                try {
                    members[m].HoursWorked = Convert.ToInt32(Console.ReadLine());
                } catch (FormatException e) {
                    m--;
                }
                members[m].CalculatePay();
            }
            var payslip = new PaySlip(year, month);
            payslip.GeneratePaySlips(members);
            payslip.GenerateSummary(members);
        }
    }

    class Staff
    {
        private float hourlyRate;
        private int hoursWorked;
        public string NameOfStaff { get; private set; }
        public float TotalPay { get; protected set; }
        public float BasicPay { get; private set; }
        public int HoursWorked {
            get {
                return hoursWorked;
            }
            set {
                if (value > 0)
                {
                    hoursWorked = value;
                }
                else
                {
                    hoursWorked = 0;
                }
            }
         
        }

        public Staff(string name, float rate)
        {
            NameOfStaff = name;
            hourlyRate = rate;
        }

        public virtual void CalculatePay() 
        {
            Console.WriteLine("Calculating pay...");
            BasicPay = HoursWorked * hourlyRate;
            TotalPay = BasicPay;
        }

        public override string ToString()
        {
            return "Name of Staff " + NameOfStaff + ", " + "hours worked" + hoursWorked;
        }
    }

    class Manager : Staff 
    {
        private const float managerHourlyRate = 40;

        public int Allowance { get; private set; }

        public Manager(string name) : base(name, managerHourlyRate)
        {

        }
        public override void CalculatePay() {
            base.CalculatePay();
            Allowance = 1000;
            if(HoursWorked > 160)
            {
                TotalPay += Allowance;
            }
        }

        public override string ToString()
        {
            return "";
        }
    }

    class Admin : Staff
    {
        private const float overtimeRate = 15.5f;
        private const float adminHourlyRate = 30f;

        public float Overtime { get; private set; }

        public Admin(string name) : base(name, adminHourlyRate)
        {

        }



        public override void CalculatePay()
        {
            base.CalculatePay();

            if(HoursWorked > 160)
            {
                Overtime = (HoursWorked - 160) * overtimeRate;
                TotalPay += Overtime;
            }
        }

        public override string ToString()
        {
            return ""; 
        }
    }

    class FileReader
    {
        public FileReader()
        {

        }

        public List<Staff> ReadFile() {
            var myStaff = new List<Staff>();
            string[] result = new string[2];
            string[] separator = { ", " };
            string path = "staff.txt";
            if (File.Exists(path))
            {
                using (var sr = new StreamReader(path))
                {
                    while (!sr.EndOfStream)
                    {
                        result = sr.ReadLine().Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        if (result[1] == "Manager")
                        {
                            myStaff.Add(new Manager(result[0]));
                        }
                        else if (result[1] == "Admin")
                        {
                            myStaff.Add(new Admin(result[0]));
                        }
                    }
                    sr.Close();
                }

            }
            else {
                Console.WriteLine("File does not exist");
            }
            return myStaff;

        }
    }

    class PaySlip 
    {
        private int year;
        private int month;
        enum MonthsOfYear { Jan = 1, Feb, Mar, Apr, May, Jun, Jul, Aug, Sep, Oct, Nov, Dec };

        public PaySlip(int payYear, int payMonth)
        {
            year = payYear;
            month = payMonth;

        }
        public void GeneratePaySlips(List<Staff> staff) {
            string path;
            foreach(var member in staff)
            {
                path = "./payslips/" + member.NameOfStaff + ".txt";
                using (var sw = new StreamWriter(path)) {
                    sw.WriteLine("PAYSLIP FOR {0} {1}", (MonthsOfYear)month, year);
                    sw.WriteLine("=============================");
                    sw.WriteLine("Name of Staff: {0}", member.NameOfStaff);
                    sw.WriteLine("Hours worked: {0}", member.HoursWorked);
                    sw.WriteLine("");
                    sw.WriteLine("Basic Pay: {0:C}", member.BasicPay);
                    if (member.GetType() == typeof(Manager))
                    {
                        sw.WriteLine("Allowance: {0:C}", ((Manager)member).Allowance);
                    }
                    else {
                        sw.WriteLine("Overtime Pay: {0:C}", ((Admin)member).Overtime);

                    }
                    sw.WriteLine("");
                    sw.WriteLine("================================");
                    sw.WriteLine("Total Pay: {0:C}", member.TotalPay);
                    sw.WriteLine("================================");
                }
            }
        }
        public void GenerateSummary(List<Staff> staff) {
            string path = "summary.txt";
            var counter = 1;
            var underWorkers =
                from Staff member in staff
                where member.HoursWorked < 10
                orderby member.NameOfStaff ascending
                select new { member.NameOfStaff, member.HoursWorked};
           // if(File.Exists(path))
            {
                using (var sw = new StreamWriter(path))
                {
                    sw.WriteLine("Staff with less than 10 hours worked");
                    sw.WriteLine("");
                    foreach(var worker in underWorkers)
                    {
                        sw.WriteLine("{0} Name of staff: {1}, Hours worked: {2}", counter, worker.NameOfStaff, worker.HoursWorked);
                        counter++;
                    }
                    sw.Close();
                }
            }
                
        }
    }
}
