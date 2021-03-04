using ActiveDs;
using BdsSoft.DirectoryServices.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Devir.DMS.DL.ActiveDirectory
{

    [DirectorySchema("user", typeof(IADsUser))]
    public class ADUser : DirectoryEntity
    {
        private object userId;

        [DirectoryAttribute("objectguid")]
        public object UserId { get { return userId; } set { userId = value; } }


       
        private string first;

        [DirectoryAttribute("givenName")]
        public string FirstName
        {
            get { return first; }
            set
            {
                if (first != value)
                {
                    first = value;
                    OnPropertyChanged("FirstName");
                }
            }
        }

        private string last;

        [DirectoryAttribute("sn")]
        public string LastName
        {
            get { return last; }
            set
            {
                if (last != value)
                {
                    last = value;
                    OnPropertyChanged("LastName");
                }
            }
        }

        private string office;

        [DirectoryAttribute("physicalDeliveryOfficeName")]
        public string Office
        {
            get { return office; }
            set
            {
                if (office != value)
                {
                    office = value;
                    OnPropertyChanged("Office");
                }
            }
        }

        private string accoutName;

        [DirectoryAttribute("sAMAccountName")]
        public string AccountName
        {
            get { return accoutName; }
            set
            {
                if (accoutName != value)
                {
                    accoutName = value;
                    OnPropertyChanged("AccountName");
                }
            }
        }

        private string department;

        [DirectoryAttribute("department")]
        public string Department
        {
            get { return department; }
            set
            {
                if (department != value)
                {
                    department = value;
                    OnPropertyChanged("Department");
                }
            }
        }

        private string position;

        [DirectoryAttribute("title")]
        public string Position
        {
            get { return position; }
            set
            {
                if (position != value)
                {
                    position = value;
                    OnPropertyChanged("Position");
                }
            }
        }

        private DateTime whenCreated;

        [DirectoryAttribute("whenCreated")]
        public DateTime WhenCreated
        {
            get { return whenCreated; }
            set
            {
                if (whenCreated != value)
                {
                    whenCreated = value;
                    OnPropertyChanged("WhenCreated");
                }
            }
        }

        private DateTime whenChanged;

        [DirectoryAttribute("whenChanged")]
        public DateTime WhenChanged
        {
            get { return whenChanged; }
            set
            {
                if (whenChanged != value)
                {
                    whenChanged = value;
                    OnPropertyChanged("WhenChanged");
                }
            }
        }

        private string email;

        [DirectoryAttribute("mail")]
        public string Email { get { return email; } set { email = value; } }
    }
}
