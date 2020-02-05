using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace WeddingPlanner.Models{

public class DashViewModel{
        public List<Wedding> AllWeddings { get; set; }
}
public class ViewWedding{
        public List<Wedding> ThisWedding { get; set; }
        public Wedding Wedding {get;set;}
}
   
    
    public class Wedding{
        public int WeddingId {get;set;}
        [Required(ErrorMessage = "You need a Mate")]
        public string WedderOne{get;set;}
        [Required(ErrorMessage = "You need a Mate")]
        public string WedderTwo { get; set; }
        [Required(ErrorMessage = "Wedding date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date {get;set;}
        public string WeddingAddress {get;set;}
        public int creator {get;set;}
        public List<RSVP>  Guest{get;set;}



    }
  
}