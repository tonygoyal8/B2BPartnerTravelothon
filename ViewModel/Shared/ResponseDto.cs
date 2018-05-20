using B2BPartnerTravelothon.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.Shared
{
    public class ResponseDto
    {
        public ObjectDto<object> objectDto;
      public List<Messages> messages=new List<Messages>();
    }
    public class ObjectDto<T> {
        public T Object { get; set; }
        public List<Messages> messages;
        public bool valid { get; set; }
        public ObjectDto()
        {
            valid = false;
            messages = new List<Messages>();
        }

    }
    public  class Messages
    {
        public Messages()
        {
            Message = "Internal Server Error";
            Type = Toaster.ERROR.ToString();
        }
        public string Type { get; set; }
        public string Message { get; set; }
    }
}