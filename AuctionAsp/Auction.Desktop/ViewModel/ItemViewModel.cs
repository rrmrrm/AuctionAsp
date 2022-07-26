using Auction.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auction.Desktop.ViewModel
{
    public class ItemViewModel: ViewModelBase
    {
        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged(); }
        }

        private String _name;

        public String Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        private String _description;
        public String Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged(); }
        }

        public Int32 _startingLicit;
        public Int32 StartingLicit
        {
            get { return _startingLicit; }
            set { _startingLicit = value; OnPropertyChanged(); }
        }

        private DateTime _dateOfClosing;
        public DateTime DateOfClosing
        {
            get { return _dateOfClosing; }
            set { _dateOfClosing = value; OnPropertyChanged(); }
        }
        /*
        private DateTime _auctionStartDate;
        public DateTime AuctionStartDate
        {
            get { return _dateOfClosing; }
            set { _dateOfClosing = value; OnPropertyChanged(); }
        }
        */
        private byte[] _picture;
        public byte[] Picture
        {
            get { return _picture; }
            set { _picture = value; OnPropertyChanged(); }
        }
        private int _categoryId;
        public int CategoryId
        {
            get { return _categoryId; }
            set { _categoryId = value; OnPropertyChanged(); }
        }
        /*
        private int _listId;

        public int ListId
        {
            get { return _listId; }
            set { _listId = value; OnPropertyChanged(); }
        }
        */

        public ItemViewModel ShallowClone()
        {
            return (ItemViewModel)this.MemberwiseClone();
        }
        /*
        public void CopyFrom (ItemViewModel rhs)
        {
            Id = rhs.Id;
            Name = rhs.Name;
            Description = rhs.Description;
            Deadline = rhs.Deadline;
            Image = rhs.Image;
            ListId = rhs.ListId;
        }*/
        /*
        public static explicit operator ItemViewModel(ItemDto dto) => new ItemViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Deadline = dto.Deadline,
            Image = dto.Image,
            ListId = dto.ListId
        };*/

        public static explicit operator ItemDTO(ItemViewModel vm) => new ItemDTO
        {
            Id = vm.Id,
            Name = vm.Name,
            Description = vm.Description,
            //AuctionStartDate, //controller állítja majd be
            //IsActive, //controller állítja majd be
            //Hirdeto, //controller állítja majd be
            DateOfClosing = vm.DateOfClosing,
            Picture = vm.Picture,
            StartingLicit = vm.StartingLicit,
            Category = new CategoryDTO {Id=vm.CategoryId}
        };

    }
}
